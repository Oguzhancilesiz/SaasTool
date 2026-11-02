using FluentValidation;
using SaasTool.Core.Enums;
using SaasTool.DTO.Billing;

namespace SaasTool.API.Validators
{
    public class CouponCreateDtoValidator : AbstractValidator<CouponCreateDto>
    {
        public CouponCreateDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DiscountType).Must(v => Enum.IsDefined(typeof(DiscountType), v)).WithMessage("Invalid discount type.");
            RuleFor(x => x.Value).GreaterThan(0);
        }
    }
}
