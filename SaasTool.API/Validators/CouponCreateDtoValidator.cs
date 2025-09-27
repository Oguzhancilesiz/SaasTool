using FluentValidation;
using SaasTool.DTO.Billing;

namespace SaasTool.API.Validators
{
    public class CouponCreateDtoValidator : AbstractValidator<CouponCreateDto>
    {
        public CouponCreateDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DiscountType).IsInEnum();
            RuleFor(x => x.Value).GreaterThan(0);
        }
    }
}
