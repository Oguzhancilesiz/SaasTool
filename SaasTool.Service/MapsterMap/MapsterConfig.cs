// SaasTool.Service/MapsterMap/MapsterConfig.cs
using Mapster;
using SaasTool.DTO.Apps;
using SaasTool.DTO.Billing;
using SaasTool.DTO.Common;
using SaasTool.DTO.Orgs;
using SaasTool.DTO.Plans;
using SaasTool.DTO.Security;
using SaasTool.Entity;
using System.Diagnostics.Contracts;

namespace SaasTool.Service.MapsterMap;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig cfg)
    {
        // Ortak: Entity -> AuditedDto
        cfg.ForType<BaseEntity, AuditedDto>()
           .IgnoreNonMapped(true);

        // Apps
        cfg.NewConfig<App, AppDto>();
        cfg.NewConfig<AppCreateDto, App>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<AppUpdateDto, App>();

        cfg.NewConfig<Feature, FeatureDto>();
        cfg.NewConfig<FeatureCreateDto, Feature>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<FeatureUpdateDto, Feature>();

        // Plans
        cfg.NewConfig<Plan, PlanDto>();
        cfg.NewConfig<PlanCreateDto, Plan>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<PlanUpdateDto, Plan>();

        cfg.NewConfig<PlanFeature, PlanFeatureDto>();
        cfg.NewConfig<PlanFeatureCreateDto, PlanFeature>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<PlanFeatureUpdateDto, PlanFeature>();

        // Orgs
        cfg.NewConfig<Organization, OrganizationDto>();
        cfg.NewConfig<OrganizationCreateDto, Organization>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<OrganizationUpdateDto, Organization>();

        cfg.NewConfig<UserOrganization, UserOrganizationDto>();
        cfg.NewConfig<UserOrganizationCreateDto, UserOrganization>()
            .Map(d => d.Id, _ => Guid.NewGuid())
            .Map(d => d.JoinedAt, _ => DateTime.UtcNow);
        cfg.NewConfig<UserOrganizationUpdateDto, UserOrganization>();

        cfg.NewConfig<Customer, CustomerDto>();
        cfg.NewConfig<CustomerCreateDto, Customer>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<CustomerUpdateDto, Customer>();

        cfg.NewConfig<AppUserProfile, AppUserProfileDto>();
        cfg.NewConfig<AppUserProfileCreateDto, AppUserProfile>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<AppUserProfileUpdateDto, AppUserProfile>();

        // Billing: Subscription
        cfg.NewConfig<SubscriptionItem, SubscriptionItemDto>();
        cfg.NewConfig<SubscriptionItemCreateDto, SubscriptionItem>()
            .Map(d => d.Id, _ => Guid.NewGuid());

        cfg.NewConfig<Subscription, SubscriptionDto>()
            .Map(d => d.Items, s => s.Items);
        cfg.NewConfig<SubscriptionCreateDto, Subscription>()
            .Map(d => d.Id, _ => Guid.NewGuid())
            .Map(d => d.SubscriptionState, _ => Core.Enums.SubscriptionStatus.Active) // başlangıç
            .Ignore(d => d.Items); // Items ayrı eklenir; ya da Mapster AfterMapping ile ekleyebilirsin
        cfg.NewConfig<SubscriptionUpdateDto, Subscription>();

        // Billing: Usage
        cfg.NewConfig<UsageRecord, UsageRecordDto>();
        cfg.NewConfig<UsageRecordCreateDto, UsageRecord>()
            .Map(d => d.Id, _ => Guid.NewGuid());

        // Billing: Coupons
        cfg.NewConfig<Coupon, CouponDto>();
        cfg.NewConfig<CouponCreateDto, Coupon>()
            .Map(d => d.Id, _ => Guid.NewGuid());
        cfg.NewConfig<CouponUpdateDto, Coupon>();

        // Billing: Invoices
        cfg.NewConfig<InvoiceLine, InvoiceLineDto>();
        cfg.NewConfig<InvoiceLineCreateDto, InvoiceLine>()
            .Map(d => d.Id, _ => Guid.NewGuid())
            .Map(d => d.LineTotal, s => s.Quantity * s.UnitPrice);

        cfg.NewConfig<Invoice, InvoiceDto>()
            .Map(d => d.Lines, s => s.Lines);
        cfg.NewConfig<InvoiceCreateDto, Invoice>()
            .Map(d => d.Id, _ => Guid.NewGuid())
            .Map(d => d.InvoiceNumber, _ => "INV-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"))
            .Map(d => d.Subtotal, _ => 0m)
            .Map(d => d.TaxTotal, _ => 0m)
            .Map(d => d.GrandTotal, _ => 0m)
            .Map(d => d.InvoiceState, _ => Core.Enums.InvoiceStatus.Draft)
            .Ignore(d => d.Lines);

        cfg.NewConfig<InvoiceUpdateDto, Invoice>();

        // Billing: Payments
        cfg.NewConfig<Payment, PaymentDto>();
        cfg.NewConfig<PaymentCreateDto, Payment>()
            .Map(d => d.Id, _ => Guid.NewGuid());

        // Security: ApiKey
        cfg.NewConfig<ApiKey, ApiKeyDto>()
            .Map(d => d.KeyLast4,
                 s => s.KeyHash != null && s.KeyHash.Length >= 4
                      ? s.KeyHash.Substring(s.KeyHash.Length - 4, 4)
                      : "****");

        cfg.NewConfig<ApiKeyCreateDto, ApiKey>()
            .Map(d => d.Id, _ => Guid.NewGuid())
            .Ignore(d => d.KeyHash); // Hash üretimi servis katmanında
    }
}
