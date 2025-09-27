using FluentValidation;
using SaasTool.DTO.Plans;
using System;

namespace SaasTool.API.Validators
{
    public class PlanCreateDtoValidator : AbstractValidator<PlanCreateDto>
    {
        public PlanCreateDtoValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Currency).IsInEnum();
            RuleFor(x => x.BillingPeriod).IsInEnum();
            RuleFor(x => x.TrialDays).GreaterThanOrEqualTo(0);
        }
    }

}
