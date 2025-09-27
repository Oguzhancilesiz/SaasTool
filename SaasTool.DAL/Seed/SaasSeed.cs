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
using System.Threading.Tasks;

namespace SaasTool.DAL.Seed
{
    public static class SaasSeed
    {
        // Deterministic GUID’ler: tekrar koştuğunda aynı kayıtlar üst üste binmez
        private static readonly Guid OrgId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid AppId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        private static readonly Guid PlanFreeId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        private static readonly Guid PlanProId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
        private static readonly Guid PlanBizId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
        private static readonly Guid FReqId = Guid.Parse("f1111111-1111-1111-1111-111111111111");
        private static readonly Guid FSeatsId = Guid.Parse("f2222222-2222-2222-2222-222222222222");
        private static readonly Guid FStorageId = Guid.Parse("f3333333-3333-3333-3333-333333333333");
        private static readonly Guid AppPlanFree = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1");
        private static readonly Guid AppPlanPro = Guid.Parse("a2a2a2a2-a2a2-a2a2-a2a2-a2a2a2a2a2a2");
        private static readonly Guid AppPlanBiz = Guid.Parse("a3a3a3a3-a3a3-a3a3-a3a3-a3a3a3a3a3a3");
        private static readonly Guid CustId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        private static readonly Guid SubFreeId = Guid.Parse("12121212-1212-1212-1212-121212121212");
        private static readonly Guid SubProId = Guid.Parse("13131313-1313-1313-1313-131313131313");
        private static readonly Guid SubItem1Id = Guid.Parse("14141414-1414-1414-1414-141414141414");
        private static readonly Guid SubItem2Id = Guid.Parse("15151515-1515-1515-1515-151515151515");
        private static readonly Guid CouponPctId = Guid.Parse("16161616-1616-1616-1616-161616161616");
        private static readonly Guid CouponFixId = Guid.Parse("17171717-1717-1717-1717-171717171717");
        private static readonly Guid SubCouponId = Guid.Parse("18181818-1818-1818-1818-181818181818");
        private static readonly Guid InvId = Guid.Parse("19191919-1919-1919-1919-191919191919");
        private static readonly Guid InvLine1Id = Guid.Parse("20202020-2020-2020-2020-202020202020");
        private static readonly Guid PayId = Guid.Parse("21212121-2121-2121-2121-212121212121");
        private static readonly Guid ApiKeyId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid UOrgId = Guid.Parse("23232323-2323-2323-2323-232323232323");
        private static readonly Guid ProfileId = Guid.Parse("24242424-2424-2424-2424-242424242424");

        private static readonly Guid RoleAdminId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        private static readonly Guid RoleUserId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");
        private static readonly Guid UserId = Guid.Parse("9999aaaa-8888-7777-6666-555544443333");

        public static async Task RunAsync(IServiceProvider sp, CancellationToken ct = default)
        {
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BaseContext>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

            // 1) Roles
            await EnsureRole(roleMgr, RoleAdminId, "Admin", ct);
            await EnsureRole(roleMgr, RoleUserId, "User", ct);



            // 2) User
            var user = await userMgr.FindByIdAsync(UserId.ToString());
            if (user is null)
            {
                user = new AppUser
                {
                    Id = UserId,
                    UserName = "founder",
                    Email = "founder@demo.local",
                    EmailConfirmed = true,
                    PhoneNumber = "+905551112233"
                };
                var res = await userMgr.CreateAsync(user, "Aa!12345");
                if (!res.Succeeded) throw new Exception("Seed user create failed: " + string.Join(",", res.Errors.Select(e => e.Description)));
                await userMgr.AddToRolesAsync(user, new[] { "Admin", "User" });
            }

            await EnsureUserInRoles(userMgr, user, "Admin", "User");


            // 3) Organization
            var org = await db.Set<Organization>().FirstOrDefaultAsync(x => x.Id == OrgId, ct);
            if (org is null)
            {
                org = new Organization { Id = OrgId, Name = "Demo Org", Slug = "demo-org" };
                db.Add(org);
            }

            // 4) App
            var app = await db.Set<App>().FirstOrDefaultAsync(x => x.Id == AppId, ct);
            if (app is null)
            {
                app = new App { Id = AppId, Code = "seridoviz", Name = "Seri Döviz", IsEnabled = true };
                db.Add(app);
            }

            // 5) Plans
            var pFree = await UpsertPlan(db, PlanFreeId, "free", "Free", 0, Currency.TRY, BillingPeriod.Monthly, trialDays: 14, ct);
            var pPro = await UpsertPlan(db, PlanProId, "pro", "Pro", 299, Currency.TRY, BillingPeriod.Monthly, trialDays: 7, ct);
            var pBiz = await UpsertPlan(db, PlanBizId, "business", "Business", 899, Currency.TRY, BillingPeriod.Monthly, trialDays: 0, ct);

            // 6) AppPlan
            await UpsertAppPlan(db, AppPlanFree, AppId, PlanFreeId, 1, ct);
            await UpsertAppPlan(db, AppPlanPro, AppId, PlanProId, 2, ct);
            await UpsertAppPlan(db, AppPlanBiz, AppId, PlanBizId, 3, ct);

            // 7) Features
            var fReq = await UpsertFeature(db, FReqId, AppId, "api.requests", "API Requests", "Dakika/Gün sınırlı istek hakkı", ct);
            var fSeat = await UpsertFeature(db, FSeatsId, AppId, "seats", "Kullanıcı Koltuğu", "Takım üyesi sayısı", ct);
            var fStorage = await UpsertFeature(db, FStorageId, AppId, "storage.mb", "Depolama", "MB cinsinden depolama", ct);

            // 8) PlanFeature limits
            await UpsertPlanFeature(db, PlanFreeId, FReqId, FeatureLimitUnit.RequestsPerDay, 100, overages: false, overUnitPrice: null, ct);
            await UpsertPlanFeature(db, PlanFreeId, FSeatsId, FeatureLimitUnit.Seats, 1, false, null, ct);
            await UpsertPlanFeature(db, PlanFreeId, FStorageId, FeatureLimitUnit.StorageMB, 100, false, null, ct);

            await UpsertPlanFeature(db, PlanProId, FReqId, FeatureLimitUnit.RequestsPerDay, 1000, true, 0.02m, ct);
            await UpsertPlanFeature(db, PlanProId, FSeatsId, FeatureLimitUnit.Seats, 5, false, null, ct);
            await UpsertPlanFeature(db, PlanProId, FStorageId, FeatureLimitUnit.StorageMB, 1024, false, null, ct);

            await UpsertPlanFeature(db, PlanBizId, FReqId, FeatureLimitUnit.RequestsPerDay, 10000, true, 0.01m, ct);
            await UpsertPlanFeature(db, PlanBizId, FSeatsId, FeatureLimitUnit.Seats, 20, false, null, ct);
            await UpsertPlanFeature(db, PlanBizId, FStorageId, FeatureLimitUnit.StorageMB, 10_240, false, null, ct);

            // 9) Customer
            var cust = await db.Set<Customer>().FirstOrDefaultAsync(x => x.Id == CustId, ct);
            if (cust is null)
            {
                cust = new Customer
                {
                    Id = CustId,
                    OrganizationId = OrgId,
                    Name = "Acme A.Ş.",
                    Email = "billing@acme.local",
                    City = "İstanbul",
                    Country = "TR",
                    BillingAddress = "Büyükdere Cad. No:1 Şişli"
                };
                db.Add(cust);
            }

            // 10) Subscriptions (Free + Pro)
            var now = DateTime.UtcNow;
            var subFree = await db.Set<Subscription>().FirstOrDefaultAsync(x => x.Id == SubFreeId, ct);
            if (subFree is null)
            {
                subFree = new Subscription
                {
                    Id = SubFreeId,
                    OrganizationId = OrgId,
                    AppId = AppId,
                    PlanId = PlanFreeId,
                    CustomerId = CustId,
                    Status = Status.Active,
                    SubscriptionState = SubscriptionStatus.Active,
                    StartsAt = now.Date.AddDays(-5),
                    TrialEndsAt = now.Date.AddDays(9), // free plan trial örnek
                    CurrentPeriodStart = new DateTime(now.Year, now.Month, 1),
                    CurrentPeriodEnd = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddSeconds(-1),
                    Provider = PaymentProvider.Manual
                };
                db.Add(subFree);
            }

            var subPro = await db.Set<Subscription>().FirstOrDefaultAsync(x => x.Id == SubProId, ct);
            if (subPro is null)
            {
                subPro = new Subscription
                {
                    Id = SubProId,
                    OrganizationId = OrgId,
                    AppId = AppId,
                    PlanId = PlanProId,
                    CustomerId = CustId,
                    Status = Status.Active,
                    SubscriptionState = SubscriptionStatus.Active,
                    StartsAt = now.Date.AddDays(-20),
                    TrialEndsAt = now.Date.AddDays(-13),
                    CurrentPeriodStart = new DateTime(now.Year, now.Month, 1),
                    CurrentPeriodEnd = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddSeconds(-1),
                    Provider = PaymentProvider.Manual
                };
                db.Add(subPro);
            }

            // 11) SubscriptionItems (koltuk ve fiyat)
            await UpsertSubItem(db, SubItem1Id, SubProId, FSeatsId, quantity: 3, unitPrice: 50, Currency.TRY, ct);
            await UpsertSubItem(db, SubItem2Id, SubProId, FStorageId, quantity: 1, unitPrice: 0, Currency.TRY, ct);

            // 12) UsageRecords (günlük istek kullanımı)
            await SeedUsage(db, subFree.Id, FReqId, PeriodUnit.Day, now, usedToday: 37, ct);
            await SeedUsage(db, subPro.Id, FReqId, PeriodUnit.Day, now, usedToday: 420, ct);

            // 13) Coupons ve SubscriptionCoupon
            await UpsertCoupon(db, CouponPctId, "WELCOME20", "Yeni kullanıcı indirimi", DiscountType.Percentage, 20, null, max: 1000, redeemBy: now.AddMonths(6), appliesToAppId: AppId, appliesToPlanId: null, ct);
            await UpsertCoupon(db, CouponFixId, "TRY100", "Sabit 100₺ indirim", DiscountType.FixedAmount, 100, Currency.TRY, max: null, redeemBy: null, appliesToAppId: AppId, appliesToPlanId: PlanProId, ct);
            await UpsertSubCoupon(db, SubCouponId, SubProId, CouponPctId, ct);

            // 14) Invoice + lines + payments (Pro abonelik için)
            var inv = await db.Set<Invoice>().FirstOrDefaultAsync(x => x.Id == InvId, ct);
            if (inv is null)
            {
                inv = new Invoice
                {
                    Id = InvId,
                    OrganizationId = OrgId,
                    CustomerId = CustId,
                    InvoiceNumber = $"INV-{now:yyyyMMdd}-0001",
                    Status = Status.Active,
                    InvoiceState = InvoiceStatus.Paid,
                    Currency = Currency.TRY,
                    Subtotal = 299,
                    TaxTotal = 0,
                    GrandTotal = 299,
                    DueDate = now.Date.AddDays(7),
                    PaidAt = now
                };
                db.Add(inv);

                db.Add(new InvoiceLine
                {
                    Id = InvLine1Id,
                    InvoiceId = InvId,
                    Description = "Pro Aylık Abonelik",
                    Quantity = 1,
                    UnitPrice = 299,
                    LineTotal = 299,
                    SubscriptionId = SubProId
                });

                db.Add(new Payment
                {
                    Id = PayId,
                    InvoiceId = InvId,
                    Provider = PaymentProvider.Manual,
                    Amount = 299,
                    Currency = Currency.TRY,
                    PaidAt = now
                });
            }

            // 15) ApiKey (hash’li)
            var apiKey = await db.Set<ApiKey>().FirstOrDefaultAsync(x => x.Id == ApiKeyId, ct);
            if (apiKey is null)
            {
                // Demo düz metin anahtar (sadece test): "demo-api-key-123"
                var plain = "demo-api-key-123";
                apiKey = new ApiKey
                {
                    Id = ApiKeyId,
                    OrganizationId = OrgId,
                    AppId = AppId,
                    Name = "Default Key",
                    KeyHash = Sha256(plain),
                    ExpiresAt = now.AddYears(1),
                    IsRevoked = false
                };
                db.Add(apiKey);
            }

            // 16) UserOrganization ve AppUserProfile
            var uorg = await db.Set<UserOrganization>().FirstOrDefaultAsync(x => x.Id == UOrgId, ct);
            if (uorg is null)
            {
                uorg = new UserOrganization { Id = UOrgId, OrganizationId = OrgId, UserId = UserId, Role = "Owner", JoinedAt = now };
                db.Add(uorg);
            }

            var profile = await db.Set<AppUserProfile>().FirstOrDefaultAsync(x => x.Id == ProfileId, ct);
            if (profile is null)
            {
                profile = new AppUserProfile
                {
                    Id = ProfileId,
                    OrganizationId = OrgId,
                    AppId = AppId,
                    UserId = UserId,
                    DisplayName = "Kurucu",
                    PreferencesJson = "{\"theme\":\"dark\"}",
                    IsBlocked = false
                };
                db.Add(profile);
            }

            await db.SaveChangesAsync(ct);
        }

        private static async Task<Plan> UpsertPlan(BaseContext db, Guid id, string code, string name, decimal price, Currency ccy, BillingPeriod period, int trialDays, CancellationToken ct)
        {
            var p = await db.Set<Plan>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (p is null)
            {
                p = new Plan { Id = id, Code = code, Name = name, Price = price, Currency = ccy, BillingPeriod = period, TrialDays = trialDays, IsPublic = true };
                db.Add(p);
            }
            return p;
        }

        private static async Task UpsertAppPlan(BaseContext db, Guid id, Guid appId, Guid planId, int order, CancellationToken ct)
        {
            var ap = await db.Set<AppPlan>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (ap is null)
            {
                ap = new AppPlan { Id = id, AppId = appId, PlanId = planId, IsEnabled = true, DisplayOrder = order };
                db.Add(ap);
            }
        }

        private static async Task<Feature> UpsertFeature(BaseContext db, Guid id, Guid appId, string code, string name, string? desc, CancellationToken ct)
        {
            var f = await db.Set<Feature>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (f is null)
            {
                f = new Feature { Id = id, AppId = appId, Code = code, Name = name, Description = desc };
                db.Add(f);
            }
            return f;
        }

        private static async Task UpsertPlanFeature(BaseContext db, Guid planId, Guid featureId, FeatureLimitUnit unit, decimal? limit, bool overages, decimal? overUnitPrice, CancellationToken ct)
        {
            var pf = await db.Set<PlanFeature>().FirstOrDefaultAsync(x => x.PlanId == planId && x.FeatureId == featureId, ct);
            if (pf is null)
            {
                pf = new PlanFeature
                {
                    Id = Guid.NewGuid(),
                    PlanId = planId,
                    FeatureId = featureId,
                    Unit = unit,
                    LimitValue = limit,
                    OveragesEnabled = overages,
                    OverageUnitPrice = overUnitPrice
                };
                db.Add(pf);
            }
            else
            {
                pf.Unit = unit;
                pf.LimitValue = limit;
                pf.OveragesEnabled = overages;
                pf.OverageUnitPrice = overUnitPrice;
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
                    Currency = ccy
                };
                db.Add(it);
            }
        }

        private static async Task SeedUsage(BaseContext db, Guid subId, Guid featureId, PeriodUnit unit, DateTime today, decimal usedToday, CancellationToken ct)
        {
            // Son 3 gün için örnek kullanım kayıtları
            for (int i = 2; i >= 0; i--)
            {
                var day = today.Date.AddDays(-i);
                var start = day;
                var end = day.AddDays(1).AddSeconds(-1);

                var exists = await db.Set<UsageRecord>()
                    .AnyAsync(x => x.SubscriptionId == subId && x.FeatureId == featureId && x.PeriodStart == start && x.PeriodEnd == end, ct);

                if (!exists)
                {
                    db.Add(new UsageRecord
                    {
                        Id = Guid.NewGuid(),
                        SubscriptionId = subId,
                        FeatureId = featureId,
                        PeriodUnit = unit,
                        PeriodStart = start,
                        PeriodEnd = end,
                        UsedValue = i == 0 ? usedToday : Math.Max(1, usedToday - (2 - i) * 50)
                    });
                }
            }
        }

        private static async Task UpsertCoupon(BaseContext db, Guid id, string code, string name, DiscountType type, decimal value, Currency? ccy,
            int? max, DateTime? redeemBy, Guid? appliesToAppId, Guid? appliesToPlanId, CancellationToken ct)
        {
            var c = await db.Set<Coupon>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (c is null)
            {
                c = new Coupon
                {
                    Id = id,
                    Code = code,
                    Name = name,
                    DiscountType = type,
                    Value = value,
                    Currency = ccy,
                    MaxRedemptions = max,
                    RedeemBy = redeemBy,
                    IsActive = true,
                    AppliesToAppId = appliesToAppId,
                    AppliesToPlanId = appliesToPlanId
                };
                db.Add(c);
            }
        }

        private static async Task UpsertSubCoupon(BaseContext db, Guid id, Guid subId, Guid couponId, CancellationToken ct)
        {
            var sc = await db.Set<SubscriptionCoupon>().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (sc is null)
            {
                sc = new SubscriptionCoupon { Id = id, SubscriptionId = subId, CouponId = couponId, AppliedAt = DateTime.UtcNow };
                db.Add(sc);
            }
        }

        private static string Sha256(string value)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return Convert.ToHexString(bytes); // uppercase hex
        }


        // SaasTool.DAL/Seed/SaasSeed.cs içinde sınıfın altına ekle
        private static async Task EnsureRole(RoleManager<AppRole> roleMgr, Guid roleId, string roleName, CancellationToken ct)
        {
            // Önce Id ile ara (deterministic GUID kullanıyoruz)
            var role = await roleMgr.FindByIdAsync(roleId.ToString());
            if (role is null)
            {
                // Yoksa isimle ara (daha önce Id farklı atandıysa)
                role = await roleMgr.FindByNameAsync(roleName);
            }

            if (role is null)
            {
                role = new AppRole
                {
                    Id = roleId,
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    Status = SaasTool.Core.Enums.Status.Active,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
                var res = await roleMgr.CreateAsync(role);
                if (!res.Succeeded)
                {
                    throw new Exception("Seed role create failed: " + string.Join(", ", res.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // İsim değiştiyse normalize et
                if (!string.Equals(role.Name, roleName, StringComparison.Ordinal))
                {
                    role.Name = roleName;
                    role.NormalizedName = roleName.ToUpperInvariant();
                    await roleMgr.UpdateAsync(role);
                }
            }
        }

        private static async Task EnsureUserInRoles(UserManager<AppUser> userMgr, AppUser user, params string[] roles)
        {
            var userRoles = await userMgr.GetRolesAsync(user);
            var missing = roles.Where(r => !userRoles.Contains(r)).ToArray();
            if (missing.Length > 0)
            {
                var res = await userMgr.AddToRolesAsync(user, missing);
                if (!res.Succeeded)
                    throw new Exception("Seed add-to-roles failed: " + string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }
    }

}
