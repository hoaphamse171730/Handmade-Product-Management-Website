using FluentValidation;
using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.Validation.Variation
{
    public class VariationForCreationDtoValidator : AbstractValidator<VariationForCreationDto>
    {
        public VariationForCreationDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Length(1, 100).WithMessage("Name must be between 1 and 100 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Name can only contain letters and spaces.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.")
                .Must(BeAValidGuid).WithMessage("CategoryId must be a valid GUID.");
        }

        private bool BeAValidGuid(string categoryId)
        {
            return Guid.TryParse(categoryId, out _);
        }
    }
}
