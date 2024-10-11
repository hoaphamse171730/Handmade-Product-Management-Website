using FluentValidation;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;

namespace HandmadeProductManagement.Validation.ProductItem
{
    public class ProductItemForUpdateDtoValidator : AbstractValidator<ProductItemForUpdateDto>
    {
        public ProductItemForUpdateDtoValidator() 
        {
            RuleFor(x => x)
                .Must(dto => dto.QuantityInStock >= 0 || dto.Price > 0)
                .WithMessage("You must provide at least one valid field for update.");

            RuleFor(x => x.QuantityInStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity in stock must be greater than or equal to 0.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");
        }
    }
}
