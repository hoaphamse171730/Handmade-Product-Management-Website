using FluentValidation;
using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;

namespace HandmadeProductManagement.Validation.ProductConfiguration
{
    public class ProductConfigurationForCreationDtoValidator : AbstractValidator<ProductConfigurationForCreationDto>
    {
        public ProductConfigurationForCreationDtoValidator()
        {
            RuleFor(x => x.ProductItemId)
                .NotEmpty().WithMessage("ProductItemId is required.")
                .Length(36).WithMessage("ProductItemId must be a valid GUID (36 characters).");

            RuleFor(x => x.VariationOptionId)
                .NotEmpty().WithMessage("VariationOptionId is required.")
                .Length(36).WithMessage("VariationOptionId must be a valid GUID (36 characters).");
        }
    }
}
