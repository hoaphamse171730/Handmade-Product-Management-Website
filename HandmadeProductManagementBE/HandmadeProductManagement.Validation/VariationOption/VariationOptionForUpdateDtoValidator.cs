using FluentValidation;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.Validation.VariationOption
{
    public class VariationOptionForUpdateDtoValidator : AbstractValidator<VariationOptionForUpdateDto>
    {
        public VariationOptionForUpdateDtoValidator() 
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required.")
                .MaximumLength(100).WithMessage("Value cannot exceed 100 characters.");

            RuleFor(x => x.QuantityInStock)
                .GreaterThanOrEqualTo(0).WithMessage("QuantityInStock must be a non-negative number.")
                .WithMessage("QuantityInStock must be greater than or equal to 0.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .WithMessage("Price must be greater than 0.");
        }
    }
}
