using FluentValidation;
using SaasTool.DTO.Billing;

namespace SaasTool.API.Validators
{
    public class InvoiceCreateDtoValidator : AbstractValidator<InvoiceCreateDto>
    {
        public InvoiceCreateDtoValidator()
        {
            RuleFor(x => x.OrganizationId).NotEmpty();
            RuleFor(x => x.Currency).IsInEnum();
            RuleForEach(x => x.Lines).ChildRules(line =>
            {
                line.RuleFor(l => l.Description).NotEmpty().MaximumLength(300);
                line.RuleFor(l => l.Quantity).GreaterThan(0);
                line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
            });
        }
    }
}
