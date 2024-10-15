using FluentValidation;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.Validation.VariationOption
{
    public class VariationOptionForUpdateDtoValidator : AbstractValidator<VariationOptionForUpdateDto>
    {
        public VariationOptionForUpdateDtoValidator()
        {
            RuleFor(x => x)
                .Must(dto => !string.IsNullOrWhiteSpace(dto.Value))
                .WithMessage("You must provide at least one valid field for update.");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required.")
                .MaximumLength(100).WithMessage("Value cannot exceed 100 characters.")
                .Matches(@"^[a-zA-Z0-9\s]*$").WithMessage("Value can only contain letters, numbers, and spaces.")
                .When(x => !string.IsNullOrWhiteSpace(x.Value));
        }
    }
}
