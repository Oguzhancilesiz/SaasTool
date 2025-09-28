using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaasTool.Core.Enums;
using SaasTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaasTool.DAL.Seed
{
    /// <summary>
    /// Idempotent mega-seed. Şema: Plan tekil (Code unique), AppPlan ile app bağları.
    /// </summary>
    public static class SaasMegaSeed
    {
        private const int RNG_SEED = 424242;

        // ---- Küçük DTO’lar ----
        private sealed class CouponDef
        {
            public string Code { get; init; } = default!;
            public string Name { get; init; } = default!;
            public DiscountType Type { get; init; }
            public decimal Value { get; init; }
            public Currency? Ccy { get; init; }
            public int? Max { get; init; }
            public DateTime? Redeem { get; init; }
            public Guid? AppId { get; init; }
            public Guid? PlanId { get; init; }
        }

        public static async Task RunAsync(IServiceProvider sp, CancellationToken ct = default)
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BaseContext>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

            var rng = new Random(RNG_SEED);
            var now = DateTime.UtcNow;

            // 0) Roller + kullanıcılar
            await EnsureRole(roleMgr, "Admin");
            await EnsureRole(roleMgr, "Owner");
            await EnsureRole(roleMgr, "User");
            await EnsureRole(roleMgr, "Billing");

            var founderId = NewGuid("user:founder");
            var founder = await userMgr.FindByIdAsync(founderId.ToString());
            if (founder is null)
            {
                founder = new AppUser
                {
                    Id = founderId,
                    UserName = "founder",
                    Email = "founder@demo.local",
                    EmailConfirmed = true,
                    PhoneNumber = "+905551112233",
                    Status = Status.Active,
                    CreatedDate = now,
                    ModifiedDate = now
                };
                MustOk(await userMgr.CreateAsync(founder, "Aa!12345"));
            }
            await EnsureUserInRoles(userMgr, founder, "Admin", "Owner", "User");

            var testUsers = new List<AppUser>();
            for (int i = 1; i <= 11; i++)
            {
                var uname = $"user{i:00}";
                var id = NewGuid($"user:{uname}");
                var u = await userMgr.FindByIdAsync(id.ToString());
                if (u is null)
                {
                    u = new AppUser
                    {
                        Id = id,
                        UserName = uname,
                        Email = $"{uname}@demo.local",
                        EmailConfirmed = i % 3 != 0,
                        PhoneNumber = $"+905550{i:0000}",
                        Status = Status.Active,
                        CreatedDate = now.AddDays(-rng.Next(10, 120)),
                        ModifiedDate = now
                    };
                    MustOk(await userMgr.CreateAsync(u, "Aa!12345"));
                }
                await EnsureUserInRoles(userMgr, u, i % 2 == 0 ? new[] { "User" } : new[] { "User", "Billing" });
                testUsers.Add(u);
            }

            // 1) Organizations
            var orgs = new (string Name, string Slug)[] {
                ("Demo Org","demo-org"),
                ("Acme Corp","acme-corp"),
                ("Fraoula Labs","fraoula-labs")
            };
            var orgDict = new Dictionary<string, Organization>();
            foreach (var (name, slug) in orgs)
            {
                var id = NewGuid($"org:{slug}");
                var row = await db.Set<Organization>().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (row is null)
                {
                    row = new Organization
                    {
                        Id = id,
                        Name = name,
                        Slug = slug,
                        Status = Status.Active,
                        CreatedDate = now.AddDays(-rng.Next(30, 300)),
                        ModifiedDate = now
                    };
                    db.Add(row);
                }
                orgDict[slug] = row;
            }
            await db.SaveChangesAsync(ct);

            // UserOrganization bağları
            await UpsertUserOrg(db, orgDict["demo-org"].Id, founder.Id, "Owner", now.AddDays(-30), ct);
            int attachIdx = 0;
            foreach (var org in orgDict.Values)
            {
                for (int i = 0; i < 5; i++)
                {
                    var u = testUsers[(attachIdx + i) % testUsers.Count];
                    await UpsertUserOrg(db, org.Id, u.Id, i == 0 ? "Admin" : "Member", now.AddDays(-rng.Next(10, 180)), ct);
                }
                attachIdx += 2;
            }
            await db.SaveChangesAsync(ct);

            // 2) Apps
            var apps = new (string Code, string Name)[] {
                ("seridoviz","Seri Döviz"),
                ("onlyik","OnlyİK")
            };
            var appDict = new Dictionary<string, App>();
            foreach (var (code, name) in apps)
            {
                var id = NewGuid($"app:{code}");
                var row = await db.Set<App>().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (row is null)
                {
                    row = new App
                    {
                        Id = id,
                        Code = code,
                        Name = name,
                        IsEnabled = true,
                        Status = Status.Active,
                        CreatedDate = now.AddDays(-rng.Next(30, 200)),
                        ModifiedDate = now
                    };
                    db.Add(row);
                }
                appDict[code] = row;
            }
            await db.SaveChangesAsync(ct);

            // 3) Plans (tekil: Code’a göre)
            var planMeta = new[]
            {
                new { Code="free",     Name="Free",     Price=0m,   Period=BillingPeriod.Monthly, Trial=14 },
                new { Code="pro",      Name="Pro",      Price=299m, Period=BillingPeriod.Monthly, Trial=7  },
                new { Code="business", Name="Business", Price=899m, Period=BillingPeriod.Monthly, Trial=0  }
            };
            var planByCode = new Dictionary<string, Plan>();
            foreach (var pm in planMeta)
            {
                var plan = await db.Set<Plan>().FirstOrDefaultAsync(x => x.Code == pm.Code, ct);
                if (plan is null)
                {
                    plan = new Plan
                    {
                        Id = NewGuid($"plan:{pm.Code}"),
                        Code = pm.Code,
                        Name = pm.Name,
                        Description = $"{pm.Name} planı",
                        Currency = Currency.TRY,
                        Price = pm.Price,
                        BillingPeriod = pm.Period,
                        IsPublic = true,
                        TrialDays = pm.Trial,
                        Status = Status.Active,
                        CreatedDate = now.AddDays(-rng.Next(10, 100)),
                        ModifiedDate = now
                    };
                    db.Add(plan);
                }
                planByCode[pm.Code] = plan;
            }
            await db.SaveChangesAsync(ct);

            // AppPlan bağları
            foreach (var app in appDict.Values)
            {
                int order = 1;
                foreach (var pm in planMeta)
                {
                    var apId = NewGuid($"appplan:{app.Code}:{pm.Code}");
                    var ap = await db.Set<AppPlan>().FirstOrDefaultAsync(x => x.Id == apId, ct);
                    if (ap is null)
                    {
                        ap = new AppPlan
                        {
                            Id = apId,
                            AppId = app.Id,
                            PlanId = planByCode[pm.Code].Id,
                            IsEnabled = true,
                            DisplayOrder = order++,
                            Status = Status.Active,
                            CreatedDate = now.AddDays(-rng.Next(5, 80)),
                            ModifiedDate = now
                        };
                        db.Add(ap);
                    }
                }
            }
            await db.SaveChangesAsync(ct);

            // 4) Features (app’e özel)
            var featureMeta = new[]
            {
                new { Code="api.requests",  Name="API Requests",   Desc="Dakika/gün sınırlı istek hakkı" },
                new { Code="seats",         Name="Kullanıcı Koltuğu", Desc="Takım üyesi sayısı" },
                new { Code="storage.mb",    Name="Depolama (MB)",  Desc="MB cinsinden depolama" },
                new { Code="webhooks",      Name="Webhooks",       Desc="Webhook bildirimleri" },
                new { Code="projects",      Name="Projeler",       Desc="Aynı hesapta proje sayısı" },
                new { Code="audit.logs",    Name="Denetim Logları",Desc="Gün başına audit log sağlama" }
            };
            var featureDict = new Dictionary<string, Feature>();
            foreach (var app in appDict.Values)
            {
                foreach (var fm in featureMeta)
                {
                    var fid = NewGuid($"feature:{app.Code}:{fm.Code}");
                    var f = await db.Set<Feature>().FirstOrDefaultAsync(x => x.Id == fid, ct);
                    if (f is null)
                    {
                        f = new Feature
                        {
                            Id = fid,
                            AppId = app.Id,
                            Code = fm.Code,
                            Name = fm.Name,
                            Description = fm.Desc,
                            Status = Status.Active,
                            CreatedDate = now.AddDays(-rng.Next(10, 60)),
                            ModifiedDate = now
                        };
                        db.Add(f);
                    }
                    featureDict[$"{app.Code}:{fm.Code}"] = f;
                }
            }
            await db.SaveChangesAsync(ct);

            // 5) PlanFeature limitleri (Plan tekil, Feature app’e bağlı)
            foreach (var app in appDict.Values)
            {
                var pFree = planByCode["free"];
                var pPro = planByCode["pro"];
                var pBiz = planByCode["business"];

                await UpsertPlanFeature(db, pFree.Id, featureDict[$"{app.Code}:api.requests"].Id, FeatureLimitUnit.RequestsPerDay, 100, false, null, ct);
                await UpsertPlanFeature(db, pFree.Id, featureDict[$"{app.Code}:seats"].Id, FeatureLimitUnit.Seats, 1, false, null, ct);
                await UpsertPlanFeature(db, pFree.Id, featureDict[$"{app.Code}:storage.mb"].Id, FeatureLimitUnit.StorageMB, 100, false, null, ct);
                await UpsertPlanFeature(db, pFree.Id, featureDict[$"{app.Code}:projects"].Id, FeatureLimitUnit.Count, 1, false, null, ct);

                await UpsertPlanFeature(db, pPro.Id, featureDict[$"{app.Code}:api.requests"].Id, FeatureLimitUnit.RequestsPerDay, 1000, true, 0.02m, ct);
                await UpsertPlanFeature(db, pPro.Id, featureDict[$"{app.Code}:seats"].Id, FeatureLimitUnit.Seats, 5, false, null, ct);
                await UpsertPlanFeature(db, pPro.Id, featureDict[$"{app.Code}:storage.mb"].Id, FeatureLimitUnit.StorageMB, 2048, false, null, ct);
                await UpsertPlanFeature(db, pPro.Id, featureDict[$"{app.Code}:projects"].Id, FeatureLimitUnit.Count, 10, false, null, ct);
                await UpsertPlanFeature(db, pPro.Id, featureDict[$"{app.Code}:webhooks"].Id, FeatureLimitUnit.Count, null, false, null, ct);

                await UpsertPlanFeature(db, pBiz.Id, featureDict[$"{app.Code}:api.requests"].Id, FeatureLimitUnit.RequestsPerDay, 10000, true, 0.01m, ct);
                await UpsertPlanFeature(db, pBiz.Id, featureDict[$"{app.Code}:seats"].Id, FeatureLimitUnit.Seats, 25, false, null, ct);
                await UpsertPlanFeature(db, pBiz.Id, featureDict[$"{app.Code}:storage.mb"].Id, FeatureLimitUnit.StorageMB, 10240, false, null, ct);
                await UpsertPlanFeature(db, pBiz.Id, featureDict[$"{app.Code}:projects"].Id, FeatureLimitUnit.Count, 100, false, null, ct);
                await UpsertPlanFeature(db, pBiz.Id, featureDict[$"{app.Code}:audit.logs"].Id, FeatureLimitUnit.Count, null, false, null, ct);
            }
            await db.SaveChangesAsync(ct);

            // 6) Customers (org başına 40)
            var cities = new[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Kayseri", "Kocaeli", "Eskişehir" };
            var countries = new[] { "TR", "DE", "GB", "NL" };
            var customersByOrg = new Dictionary<Guid, List<Customer>>();
            foreach (var org in orgDict.Values)
            {
                var list = new List<Customer>();
                for (int i = 1; i <= 40; i++)
                {
                    var email = $"billing{i:00}@{org.Slug}.local";
                    var id = NewGuid($"customer:{org.Slug}:{email}");
                    var row = await db.Set<Customer>().FirstOrDefaultAsync(x => x.Id == id, ct);
                    if (row is null)
                    {
                        row = new Customer
                        {
                            Id = id,
                            OrganizationId = org.Id,
                            Name = $"{org.Name} Customer {i:00}",
                            Email = email,
                            TaxNumber = $"TN-{rng.Next(1000000, 9999999)}",
                            BillingAddress = $"{rng.Next(1, 200)}. Cad. No:{rng.Next(1, 50)}",
                            Country = countries[rng.Next(countries.Length)],
                            City = cities[rng.Next(cities.Length)],
                            Status = Status.Active,
                            CreatedDate = now.AddDays(-rng.Next(30, 365)),
                            ModifiedDate = now
                        };
                        db.Add(row);
                    }
                    list.Add(row);
                }
                customersByOrg[org.Id] = list;
            }
            await db.SaveChangesAsync(ct);

            // 7) Subscriptions (~100+)
            var subs = new List<Subscription>();
            foreach (var org in orgDict.Values)
            {
                var custs = customersByOrg[org.Id];
                for (int i = 0; i < custs.Count; i++)
                {
                    var app = i % 2 == 0 ? appDict["seridoviz"] : appDict["onlyik"];
                    var tier = i % 3 == 0 ? "free" : (i % 3 == 1 ? "pro" : "business");
                    var plan = planByCode[tier];

                    var start = now.Date.AddMonths(-rng.Next(0, 8)).AddDays(-rng.Next(0, 25));
                    var currStart = new DateTime(now.Year, now.Month, 1);
                    var currEnd = currStart.AddMonths(1).AddSeconds(-1);

                    var sid = NewGuid($"sub:{org.Slug}:{custs[i].Email}:{app.Code}:{tier}");
                    var sub = await db.Set<Subscription>().FirstOrDefaultAsync(x => x.Id == sid, ct);
                    if (sub is null)
                    {
                        sub = new Subscription
                        {
                            Id = sid,
                            OrganizationId = org.Id,
                            AppId = app.Id,
                            PlanId = plan.Id,
                            CustomerId = custs[i].Id,
                            Status = Status.Active,
                            SubscriptionState = SubscriptionStatus.Active,
                            StartsAt = start,
                            TrialEndsAt = tier == "free" ? start.AddDays(14) : tier == "pro" ? start.AddDays(7) : null,
                            CurrentPeriodStart = currStart,
                            CurrentPeriodEnd = currEnd,
                            CancelAtPeriodEnd = false,
                            Provider = PaymentProvider.Manual,
                            CreatedDate = start,
                            ModifiedDate = now
                        };
                        db.Add(sub);
                    }
                    subs.Add(sub);

                    // Items
                    await UpsertSubItem(db,
                        NewGuid($"subitem:{sub.Id}:seats"),
                        sub.Id, featureDict[$"{app.Code}:seats"].Id,
                        quantity: tier == "free" ? 1 : tier == "pro" ? rng.Next(3, 6) : rng.Next(10, 26),
                        unitPrice: tier == "free" ? 0 : tier == "pro" ? 50 : 40,
                        Currency.TRY, ct);

                    await UpsertSubItem(db,
                        NewGuid($"subitem:{sub.Id}:storage"),
                        sub.Id, featureDict[$"{app.Code}:storage.mb"].Id,
                        quantity: 1, unitPrice: 0, Currency.TRY, ct);
                }
            }
            await db.SaveChangesAsync(ct);

            // 8) UsageRecords (30 gün, yalnız api.requests)
            foreach (var sub in subs)
            {
                var appCode = appDict.Values.First(a => a.Id == sub.AppId).Code;
                var fReq = featureDict[$"{appCode}:api.requests"];

                decimal limit = await db.Set<PlanFeature>()
                    .Where(pf => pf.PlanId == sub.PlanId && pf.FeatureId == fReq.Id)
                    .Select(pf => pf.LimitValue)
                    .FirstOrDefaultAsync(ct) ?? 100m;

                for (int i = 29; i >= 0; i--)
                {
                    var day = now.Date.AddDays(-i);
                    var start = day;
                    var end = day.AddDays(1).AddSeconds(-1);

                    bool exists = await db.Set<UsageRecord>()
                        .AnyAsync(x => x.SubscriptionId == sub.Id && x.FeatureId == fReq.Id && x.PeriodStart == start && x.PeriodEnd == end, ct);
                    if (!exists)
                    {
                        var usedDec = limit * (0.25m + (decimal)rng.NextDouble() * 0.8m);
                        var used = Math.Max(0, (int)usedDec);
                        db.Add(new UsageRecord
                        {
                            Id = NewGuid($"usage:{sub.Id}:{fReq.Id}:{day:yyyyMMdd}"),
                            SubscriptionId = sub.Id,
                            FeatureId = fReq.Id,
                            PeriodUnit = PeriodUnit.Day,
                            PeriodStart = start,
                            PeriodEnd = end,
                            UsedValue = used,
                            Status = Status.Active,
                            CreatedDate = end,
                            ModifiedDate = end
                        });
                    }
                }
            }
            await db.SaveChangesAsync(ct);

            // 9) Coupons
            var couponDefs = new List<CouponDef>
            {
                new CouponDef { Code="WELCOME20", Name="Yeni kullanıcı indirimi", Type=DiscountType.Percentage, Value=20m,  Ccy=null,            Max=1000, Redeem=now.AddMonths(6),  AppId=null,                          PlanId=null },
                new CouponDef { Code="TRY100",    Name="Sabit 100₺ indirim",     Type=DiscountType.FixedAmount, Value=100m, Ccy=Currency.TRY,   Max=null, Redeem=null,              AppId=appDict["seridoviz"].Id,      PlanId=planByCode["pro"].Id },
                new CouponDef { Code="PRO15",     Name="%15 Pro",                Type=DiscountType.Percentage,  Value=15m,  Ccy=null,            Max=500,  Redeem=now.AddMonths(3),  AppId=null,                          PlanId=planByCode["pro"].Id },
                new CouponDef { Code="BIZ250",    Name="Business 250₺",          Type=DiscountType.FixedAmount, Value=250m, Ccy=Currency.TRY,   Max=200,  Redeem=now.AddMonths(2),  AppId=null,                          PlanId=planByCode["business"].Id },
                new CouponDef { Code="TRIAL7",    Name="7 gün deneme",           Type=DiscountType.Percentage,  Value=100m, Ccy=null,            Max=10000,Redeem=now.AddMonths(12),AppId=null,                          PlanId=null },
                new CouponDef { Code="DEV50",     Name="Developer %50",          Type=DiscountType.Percentage,  Value=50m,  Ccy=null,            Max=50,   Redeem=now.AddDays(60),   AppId=appDict["seridoviz"].Id,       PlanId=null }
            };

            var couponDict = new Dictionary<string, Coupon>();
            foreach (var c in couponDefs)
            {
                var id = NewGuid($"coupon:{c.Code}");
                var row = await db.Set<Coupon>().FirstOrDefaultAsync(x => x.Id == id, ct);
                if (row is null)
                {
                    row = new Coupon
                    {
                        Id = id,
                        Code = c.Code,
                        Name = c.Name,
                        DiscountType = c.Type,
                        Value = c.Value,
                        Currency = c.Ccy,
                        MaxRedemptions = c.Max,
                        RedeemBy = c.Redeem,
                        IsActive = true,
                        AppliesToAppId = c.AppId,
                        AppliesToPlanId = c.PlanId,
                        Status = Status.Active,
                        CreatedDate = now.AddDays(-rng.Next(10, 80)),
                        ModifiedDate = now
                    };
                    db.Add(row);
                }
                couponDict[c.Code] = row;
            }
            await db.SaveChangesAsync(ct);

            // SubscriptionCoupon: aktif aboneliklerin ~%30’u
            foreach (var sub in subs.Where(s => s.SubscriptionState == SubscriptionStatus.Active))
            {
                if (rng.NextDouble() < 0.30)
                {
                    var pick = couponDict.Values.ElementAt(rng.Next(couponDict.Count));
                    var scId = NewGuid($"subcoupon:{sub.Id}:{pick.Code}");
                    bool exists = await db.Set<SubscriptionCoupon>().AnyAsync(x => x.Id == scId, ct);
                    if (!exists)
                    {
                        db.Add(new SubscriptionCoupon
                        {
                            Id = scId,
                            SubscriptionId = sub.Id,
                            CouponId = pick.Id,
                            AppliedAt = now.AddDays(-rng.Next(1, 60)),
                            Status = Status.Active,
                            CreatedDate = now.AddDays(-rng.Next(1, 60)),
                            ModifiedDate = now
                        });
                    }
                }
            }
            await db.SaveChangesAsync(ct);

            // 10) Invoices + Lines + Payments
            foreach (var (orgId, custs) in customersByOrg)
            {
                foreach (var cust in custs)
                {
                    int invCount = rng.Next(2, 5);
                    for (int i = 1; i <= invCount; i++)
                    {
                        var invId = NewGuid($"invoice:{cust.Id}:{i}");
                        var inv = await db.Set<Invoice>().FirstOrDefaultAsync(x => x.Id == invId, ct);
                        if (inv is null)
                        {
                            var currency = Currency.TRY;
                            decimal subtotal = 0;

                            var custSubs = subs.Where(s => s.CustomerId == cust.Id).ToList();
                            int lineCount = rng.Next(1, Math.Max(2, custSubs.Count == 0 ? 3 : 5));
                            var lines = new List<InvoiceLine>();

                            for (int l = 0; l < lineCount; l++)
                            {
                                Subscription? pickSub = custSubs.Count > 0 ? custSubs[rng.Next(custSubs.Count)] : null;
                                decimal unit = pickSub is null
                                    ? rng.Next(50, 300)
                                    : (await db.Set<Plan>().Where(p => p.Id == pickSub.PlanId).Select(p => p.Price).FirstAsync(ct));

                                var qty = rng.Next(1, 2);
                                var total = unit * qty;
                                subtotal += total;

                                lines.Add(new InvoiceLine
                                {
                                    Id = NewGuid($"invoiceline:{invId}:{l}"),
                                    InvoiceId = invId,
                                    Description = pickSub is null
                                        ? "Hizmet Bedeli"
                                        : $"{appDict.Values.First(a => a.Id == pickSub.AppId).Name} {(await db.Set<Plan>().Where(p => p.Id == pickSub.PlanId).Select(p => p.Name).FirstAsync(ct))} Aylık Abonelik",
                                    Quantity = qty,
                                    UnitPrice = unit,
                                    LineTotal = total,
                                    SubscriptionId = pickSub?.Id,
                                    Status = Status.Active,
                                    CreatedDate = now.AddDays(-rng.Next(5, 90)),
                                    ModifiedDate = now
                                });
                            }

                            var grand = subtotal;
                            bool paid = rng.NextDouble() < 0.85;

                            inv = new Invoice
                            {
                                Id = invId,
                                OrganizationId = orgId,
                                CustomerId = cust.Id,
                                InvoiceNumber = $"INV-{now:yyyyMMdd}-{cust.AutoID:0000}-{i:00}",
                                InvoiceState = paid ? InvoiceStatus.Paid : InvoiceStatus.Open,
                                Currency = currency,
                                Subtotal = subtotal,
                                TaxTotal = 0,
                                GrandTotal = grand,
                                DueDate = now.Date.AddDays(rng.Next(5, 20)),
                                PaidAt = paid ? now.AddDays(-rng.Next(0, 5)) : (DateTime?)null,
                                Provider = PaymentProvider.Manual,
                                Status = Status.Active,
                                CreatedDate = now.AddDays(-rng.Next(10, 120)),
                                ModifiedDate = now
                            };
                            db.Add(inv);
                            db.AddRange(lines);

                            if (paid)
                            {
                                db.Add(new Payment
                                {
                                    Id = NewGuid($"payment:{invId}"),
                                    InvoiceId = invId,
                                    Provider = PaymentProvider.Manual,
                                    ProviderPaymentId = null,
                                    Amount = grand,
                                    Currency = currency,
                                    PaidAt = inv.PaidAt ?? now,
                                    Status = Status.Active,
                                    CreatedDate = inv.PaidAt ?? now,
                                    ModifiedDate = inv.PaidAt ?? now
                                });
                            }
                        }
                    }
                }
            }
            await db.SaveChangesAsync(ct);

            // 11) ApiKeys: Org x App başına 2
            foreach (var org in orgDict.Values)
            {
                foreach (var app in appDict.Values)
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        var id = NewGuid($"apikey:{org.Slug}:{app.Code}:{i}");
                        bool exists = await db.Set<ApiKey>().AnyAsync(x => x.Id == id, ct);
                        if (!exists)
                        {
                            var plain = $"{app.Code}-{org.Slug}-demo-{100 + i}";
                            db.Add(new ApiKey
                            {
                                Id = id,
                                OrganizationId = org.Id,
                                AppId = app.Id,
                                Name = i == 1 ? "Default Key" : $"Service Key {i}",
                                KeyHash = Sha256(plain),
                                ExpiresAt = now.AddMonths(3 + 3 * i),
                                IsRevoked = false,
                                Status = Status.Active,
                                CreatedDate = now.AddDays(-rng.Next(3, 30)),
                                ModifiedDate = now
                            });
                        }
                    }
                }
            }
            await db.SaveChangesAsync(ct);

            // 12) AppUserProfile: app başına 6
            foreach (var app in appDict.Values)
            {
                var owners = new[] { founder }.Concat(testUsers.OrderBy(_ => rng.Next()).Take(5)).ToList();
                int i = 0;
                foreach (var u in owners)
                {
                    var org = orgDict.Values.ElementAt(i % orgDict.Count);
                    var pid = NewGuid($"profile:{org.Slug}:{app.Code}:{u.UserName}");
                    bool exists = await db.Set<AppUserProfile>().AnyAsync(x => x.Id == pid, ct);
                    if (!exists)
                    {
                        db.Add(new AppUserProfile
                        {
                            Id = pid,
                            OrganizationId = org.Id,
                            AppId = app.Id,
                            UserId = u.Id,
                            DisplayName = u.UserName.ToUpperInvariant(),
                            PreferencesJson = "{\"theme\":\"dark\",\"locale\":\"tr-TR\"}",
                            IsBlocked = false,
                            Status = Status.Active,
                            CreatedDate = now.AddDays(-rng.Next(10, 90)),
                            ModifiedDate = now
                        });
                    }
                    i++;
                }
            }
            await db.SaveChangesAsync(ct);

            // 13) WebhookEventLog: 60
            var providers = new[] { PaymentProvider.Manual, PaymentProvider.Stripe, PaymentProvider.Iyzico, PaymentProvider.PayTR };
            var events = new[] { "invoice.paid", "invoice.payment_failed", "subscription.created", "subscription.canceled", "customer.updated", "payment.refunded" };
            for (int i = 0; i < 60; i++)
            {
                var wid = NewGuid($"webhook:{i:000}");
                bool exists = await db.Set<WebhookEventLog>().AnyAsync(x => x.Id == wid, ct);
                if (!exists)
                {
                    var pr = providers[rng.Next(providers.Length)];
                    var et = events[rng.Next(events.Length)];
                    bool processed = rng.NextDouble() < 0.9;
                    db.Add(new WebhookEventLog
                    {
                        Id = wid,
                        Provider = pr,
                        EventType = et,
                        PayloadJson = "{\"ok\":true}",
                        IsProcessed = processed,
                        ProcessedAt = processed ? now.AddMinutes(-rng.Next(1, 5000)) : null,
                        ProcessNote = processed ? "Handled" : null,
                        Status = Status.Active,
                        CreatedDate = now.AddDays(-rng.Next(5, 120)),
                        ModifiedDate = now
                    });
                }
            }
            await db.SaveChangesAsync(ct);
        }

        // ----------------- Helpers -----------------
        private static async Task EnsureRole(RoleManager<AppRole> roleMgr, string roleName)
        {
            var id = NewGuid($"role:{roleName}");
            var role = await roleMgr.FindByIdAsync(id.ToString()) ?? await roleMgr.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new AppRole
                {
                    Id = id,
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Status = Status.Active,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
                MustOk(await roleMgr.CreateAsync(role));
            }
            else
            {
                if (!string.Equals(role.Name, roleName, StringComparison.Ordinal))
                {
                    role.Name = roleName;
                    role.NormalizedName = roleName.ToUpperInvariant();
                    MustOk(await roleMgr.UpdateAsync(role));
                }
            }
        }

        private static async Task EnsureUserInRoles(UserManager<AppUser> userMgr, AppUser user, params string[] roles)
        {
            var current = await userMgr.GetRolesAsync(user);
            var missing = roles.Where(r => !current.Contains(r)).ToArray();
            if (missing.Length > 0) MustOk(await userMgr.AddToRolesAsync(user, missing));
        }

        private static void MustOk(IdentityResult res)
        {
            if (!res.Succeeded)
                throw new Exception("Identity error: " + string.Join(", ", res.Errors.Select(e => e.Description)));
        }

        private static async Task UpsertPlanFeature(BaseContext db, Guid planId, Guid featureId, FeatureLimitUnit unit, decimal? limit, bool overages, decimal? overUnitPrice, CancellationToken ct)
        {
            var pf = await db.Set<PlanFeature>().FirstOrDefaultAsync(x => x.PlanId == planId && x.FeatureId == featureId, ct);
            if (pf is null)
            {
                pf = new PlanFeature
                {
                    Id = NewGuid($"planfeature:{planId}:{featureId}"),
                    PlanId = planId,
                    FeatureId = featureId,
                    Unit = unit,
                    LimitValue = limit,
                    OveragesEnabled = overages,
                    OverageUnitPrice = overUnitPrice,
                    Status = Status.Active,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
                db.Add(pf);
            }
            else
            {
                pf.Unit = unit; pf.LimitValue = limit; pf.OveragesEnabled = overages; pf.OverageUnitPrice = overUnitPrice; pf.ModifiedDate = DateTime.UtcNow;
            }
        }

        private static async Task UpsertSubItem(BaseContext db, Guid id, Guid subId, Guid? featureId, decimal quantity, decimal unitPrice, Currency ccy, CancellationToken ct)
        {
            var it = await db.Set<SubscriptionItem>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (it is null)
            {
                it = new SubscriptionItem
                {
                    Id = id,
                    SubscriptionId = subId,
                    FeatureId = featureId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Currency = ccy,
                    Status = Status.Active,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
                db.Add(it);
            }
            else
            {
                it.Quantity = quantity; it.UnitPrice = unitPrice; it.Currency = ccy; it.ModifiedDate = DateTime.UtcNow;
            }
        }

        private static async Task UpsertUserOrg(BaseContext db, Guid orgId, Guid userId, string role, DateTime joinedAt, CancellationToken ct)
        {
            var id = NewGuid($"userorg:{orgId}:{userId}");
            var row = await db.Set<UserOrganization>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (row is null)
            {
                row = new UserOrganization
                {
                    Id = id,
                    OrganizationId = orgId,
                    UserId = userId,
                    Role = role,
                    JoinedAt = joinedAt,
                    Status = Status.Active,
                    CreatedDate = joinedAt,
                    ModifiedDate = DateTime.UtcNow
                };
                db.Add(row);
            }
            else
            {
                row.Role = role; row.ModifiedDate = DateTime.UtcNow;
            }
        }

        private static Guid NewGuid(string input)
        {
            using var md5 = MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new Guid(bytes);
        }

        private static string Sha256(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes);
        }
    }
}
