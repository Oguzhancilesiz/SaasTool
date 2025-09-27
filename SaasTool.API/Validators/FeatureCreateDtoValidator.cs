using FluentValidation;
using SaasTool.DTO.Apps;
using System;

namespace SaasTool.API.Validators
{
    public class FeatureCreateDtoValidator : AbstractValidator<FeatureCreateDto>
    {
        public FeatureCreateDtoValidator()
        {
            RuleFor(x => x.AppId).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        }
    }

}
