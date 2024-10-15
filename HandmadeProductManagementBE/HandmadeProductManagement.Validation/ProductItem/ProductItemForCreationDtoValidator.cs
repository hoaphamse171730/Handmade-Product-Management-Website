using FluentValidation;
using HandmadeProductManagement.ModelViews.ProductItemModelViews;

namespace HandmadeProductManagement.Validation.ProductItem
{
    public class ProductItemForCreationDtoValidator : AbstractValidator<ProductItemForCreationDto>
    {
        public ProductItemForCreationDtoValidator() 
        {
            RuleFor(x => x.QuantityInStock)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Quantity in stock must be greater than or equal to 0.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");
        }
    }
}
