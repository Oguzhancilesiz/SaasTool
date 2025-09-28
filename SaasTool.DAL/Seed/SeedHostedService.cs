using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaasTool.Core.Abstracts;
using SaasTool.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaasTool.DAL.Seed
{
    public class SeedHostedService : IHostedService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<SeedHostedService> _logger;
        public SeedHostedService(IServiceProvider sp, ILogger<SeedHostedService> logger) { _sp = sp; _logger = logger; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _sp.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // Zaten data varsa çık
            var hasOrg = await (await uow.Repository<Organization>().GetAllActives())
     .AnyAsync(cancellationToken);
            if (hasOrg) return;

            var org = new Organization { Id = Guid.NewGuid(), Name = "Froaula Labs", Slug = "froaula", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            await uow.Repository<Organization>().AddAsync(org);

            var app = new App { Id = Guid.NewGuid(), Code = "CORE", Name = "Core Suite", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow, IsEnabled = true };
            await uow.Repository<App>().AddAsync(app);

            var pFree = new Plan { Id = Guid.NewGuid(), Code = "FREE", Name = "Free", Price = 0, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            var pPro = new Plan { Id = Guid.NewGuid(), Code = "PRO", Name = "Pro", Price = 299, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            var pBiz = new Plan { Id = Guid.NewGuid(), Code = "BIZ", Name = "Business", Price = 999, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow };
            await uow.Repository<Plan>().AddAsync(pFree);
            await uow.Repository<Plan>().AddAsync(pPro);
            await uow.Repository<Plan>().AddAsync(pBiz);

            await uow.Repository<AppPlan>().AddAsync(new AppPlan { Id = Guid.NewGuid(), AppId = app.Id, PlanId = pFree.Id, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
            await uow.Repository<AppPlan>().AddAsync(new AppPlan { Id = Guid.NewGuid(), AppId = app.Id, PlanId = pPro.Id, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });
            await uow.Repository<AppPlan>().AddAsync(new AppPlan { Id = Guid.NewGuid(), AppId = app.Id, PlanId = pBiz.Id, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow });

            // 10 müşteri
            var customers = Enumerable.Range(1, 10).Select(i => new Customer
            {
                Id = Guid.NewGuid(),
                OrganizationId = org.Id,
                Name = $"Müşteri {i}",
                Email = $"c{i}@demo.local",
                CreatedDate = DateTime.UtcNow.AddDays(-20 + i),
                ModifiedDate = DateTime.UtcNow
            }).ToList();
            foreach (var c in customers) await uow.Repository<Customer>().AddAsync(c);

            // 20 abonelik + birkaç ödeme
            var rnd = new Random();
            foreach (var c in customers)
            {
                var plan = new[] { pFree, pPro, pBiz }[rnd.Next(0, 3)];
                var sub = new Subscription
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = org.Id,
                    AppId = app.Id,
                    PlanId = plan.Id,
                    CustomerId = c.Id,
                    SubscriptionState = Core.Enums.SubscriptionStatus.Active,
                    StartsAt = DateTime.UtcNow.AddMonths(-3),
                    CurrentPeriodStart = DateTime.UtcNow.AddDays(-15),
                    CurrentPeriodEnd = DateTime.UtcNow.AddDays(15),
                    CreatedDate = DateTime.UtcNow.AddDays(-rnd.Next(1, 90)),
                    ModifiedDate = DateTime.UtcNow
                };
                await uow.Repository<Subscription>().AddAsync(sub);

                if (plan.Price > 0)
                {
                    var inv = new Invoice
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = org.Id,
                        CustomerId = c.Id,
                        InvoiceNumber = "INV-" + DateTime.UtcNow.Ticks,
                        InvoiceState = Core.Enums.InvoiceStatus.Paid,
                        Currency = Core.Enums.Currency.TRY,
                        Subtotal = plan.Price,
                        TaxTotal = 0,
                        GrandTotal = plan.Price,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        PaidAt = DateTime.UtcNow.AddDays(-rnd.Next(1, 20))
                    };
                    await uow.Repository<Invoice>().AddAsync(inv);
                    await uow.Repository<InvoiceLine>().AddAsync(new InvoiceLine
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = inv.Id,
                        Description = plan.Name,
                        Quantity = 1,
                        UnitPrice = plan.Price,
                        LineTotal = plan.Price
                    });
                    await uow.Repository<Payment>().AddAsync(new Payment
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = inv.Id,
                        Amount = plan.Price,
                        Currency = Core.Enums.Currency.TRY,
                        PaidAt = inv.PaidAt!.Value,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow
                    });
                }
            }

            await uow.SaveChangesAsync();
            _logger.LogInformation("Seed completed with org {Org}", org.Name);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
