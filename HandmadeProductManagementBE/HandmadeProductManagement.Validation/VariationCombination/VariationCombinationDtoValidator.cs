using FluentValidation;
using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;

namespace HandmadeProductManagement.Validation.VariationCombination
{
    public class VariationCombinationDtoValidator : AbstractValidator<VariationCombinationDto>
    {
        public VariationCombinationDtoValidator()
        {
            // Price must be greater than 0
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            // QuantityInStock must be greater than or equal to 0
            RuleFor(x => x.QuantityInStock)
                .GreaterThanOrEqualTo(0).WithMessage("QuantityInStock cannot be less than 0.");
        }
    }
}
