using FluentValidation;
using SaasTool.DTO.Billing;
using System;

namespace SaasTool.API.Validators
{
    public class SubscriptionCreateDtoValidator : AbstractValidator<SubscriptionCreateDto>
    {
        public SubscriptionCreateDtoValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.AppId).NotEmpty();
            RuleFor(x => x.PlanId).NotEmpty();
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.Quantity).GreaterThanOrEqualTo(0);
                item.RuleFor(i => i.Currency).IsInEnum();
            });
        }
    }
}
