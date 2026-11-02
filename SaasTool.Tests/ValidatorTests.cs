using SaasTool.API.Validators;
using SaasTool.Core.Enums;
using SaasTool.DTO.Billing;
using SaasTool.DTO.Plans;

namespace SaasTool.Tests
{
    public class PlanCreateDtoValidatorTests
    {
        private readonly PlanCreateDtoValidator _validator = new();

        private static PlanCreateDto ValidPlan() => new()
        {
            Code = "pro",
            Name = "Pro",
            Currency = Currency.TRY,
            Price = 299m,
            BillingPeriod = BillingPeriod.Monthly,
            TrialDays = 14
        };

        [Fact]
        public void Accepts_valid_plan()
        {
            Assert.True(_validator.Validate(ValidPlan()).IsValid);
        }

        [Fact]
        public void Accepts_free_plan()
        {
            Assert.True(_validator.Validate(ValidPlan() with { Price = 0m }).IsValid);
        }

        [Fact]
        public void Rejects_negative_price()
        {
            var result = _validator.Validate(ValidPlan() with { Price = -1m });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlanCreateDto.Price));
        }

        [Fact]
        public void Rejects_negative_trial_days()
        {
            var result = _validator.Validate(ValidPlan() with { TrialDays = -1 });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlanCreateDto.TrialDays));
        }

        [Fact]
        public void Rejects_empty_or_too_long_code()
        {
            Assert.False(_validator.Validate(ValidPlan() with { Code = "" }).IsValid);
            Assert.False(_validator.Validate(ValidPlan() with { Code = new string('x', 101) }).IsValid);
        }

        [Fact]
        public void Rejects_undefined_currency_and_billing_period()
        {
            var result = _validator.Validate(ValidPlan() with
            {
                Currency = (Currency)99,
                BillingPeriod = (BillingPeriod)99
            });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlanCreateDto.Currency));
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(PlanCreateDto.BillingPeriod));
        }
    }

    public class CouponCreateDtoValidatorTests
    {
        private readonly CouponCreateDtoValidator _validator = new();

        private static CouponCreateDto ValidCoupon() => new()
        {
            Code = "WELCOME10",
            Name = "Welcome 10%",
            DiscountType = (int)DiscountType.Percentage,
            Value = 10m
        };

        [Fact]
        public void Accepts_valid_coupon()
        {
            Assert.True(_validator.Validate(ValidCoupon()).IsValid);
        }

        [Fact]
        public void Rejects_undefined_discount_type()
        {
            var result = _validator.Validate(ValidCoupon() with { DiscountType = 99 });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CouponCreateDto.DiscountType));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Rejects_non_positive_value(int value)
        {
            var result = _validator.Validate(ValidCoupon() with { Value = value });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CouponCreateDto.Value));
        }

        [Fact]
        public void Rejects_missing_code_and_name()
        {
            var result = _validator.Validate(ValidCoupon() with { Code = "", Name = "" });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CouponCreateDto.Code));
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CouponCreateDto.Name));
        }
    }

    public class SubscriptionCreateDtoValidatorTests
    {
        private readonly SubscriptionCreateDtoValidator _validator = new();

        private static SubscriptionCreateDto ValidSubscription() => new()
        {
            OrganizationId = Guid.NewGuid(),
            AppId = Guid.NewGuid(),
            PlanId = Guid.NewGuid()
        };

        [Fact]
        public void Accepts_subscription_without_items()
        {
            Assert.True(_validator.Validate(ValidSubscription()).IsValid);
        }

        [Fact]
        public void Rejects_missing_ids()
        {
            var result = _validator.Validate(new SubscriptionCreateDto());

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubscriptionCreateDto.OrganizationId));
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubscriptionCreateDto.AppId));
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(SubscriptionCreateDto.PlanId));
        }

        // Regression: Currency is an int on the DTO, and IsInEnum() rejected every value.
        [Fact]
        public void Accepts_item_with_defined_currency()
        {
            var dto = ValidSubscription() with
            {
                Items = [new SubscriptionItemCreateDto { Quantity = 1m, UnitPrice = 10m, Currency = (int)Currency.USD }]
            };

            Assert.True(_validator.Validate(dto).IsValid);
        }

        [Fact]
        public void Rejects_item_with_undefined_currency()
        {
            var dto = ValidSubscription() with
            {
                Items = [new SubscriptionItemCreateDto { Quantity = 1m, Currency = 99 }]
            };

            Assert.False(_validator.Validate(dto).IsValid);
        }

        [Fact]
        public void Rejects_item_with_negative_quantity()
        {
            var dto = ValidSubscription() with
            {
                Items = [new SubscriptionItemCreateDto { Quantity = -1m, Currency = (int)Currency.USD }]
            };

            Assert.False(_validator.Validate(dto).IsValid);
        }
    }

    public class InvoiceCreateDtoValidatorTests
    {
        private readonly InvoiceCreateDtoValidator _validator = new();

        private static InvoiceCreateDto ValidInvoice() => new()
        {
            OrganizationId = Guid.NewGuid(),
            Currency = (int)Currency.EUR,
            Lines = [new InvoiceLineCreateDto { Description = "Pro plan", Quantity = 1m, UnitPrice = 49m }]
        };

        // Regression: Currency is an int on the DTO, and IsInEnum() rejected every value.
        [Fact]
        public void Accepts_valid_invoice()
        {
            Assert.True(_validator.Validate(ValidInvoice()).IsValid);
        }

        [Fact]
        public void Rejects_undefined_currency()
        {
            var result = _validator.Validate(ValidInvoice() with { Currency = 0 });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(InvoiceCreateDto.Currency));
        }

        [Fact]
        public void Rejects_line_with_zero_quantity()
        {
            var dto = ValidInvoice() with
            {
                Lines = [new InvoiceLineCreateDto { Description = "x", Quantity = 0m, UnitPrice = 1m }]
            };

            Assert.False(_validator.Validate(dto).IsValid);
        }
    }
}
