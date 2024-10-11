using FluentValidation;
using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.Validation.Variation
{
    public class VariationForUpdateDtoValidator : AbstractValidator<VariationForUpdateDto>
    {
        public VariationForUpdateDtoValidator() 
        {
            RuleFor(x => x)
                .Must(dto => !string.IsNullOrWhiteSpace(dto.Name))
                .WithMessage("You must provide at least one valid field for update.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Length(1, 100).WithMessage("Name must be between 1 and 100 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces.");
        }

        private bool BeAValidGuid(string categoryId)
        {
            return Guid.TryParse(categoryId, out _);
        }
    }
}
