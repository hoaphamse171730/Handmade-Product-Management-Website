using FluentValidation;
using HandmadeProductManagement.ModelViews.CartItemModelViews;

namespace HandmadeProductManagement.Validation.CartItem
{
    public class CartItemForCreationDtoValidator : AbstractValidator<CartItemForCreationDto>
    {
        public CartItemForCreationDtoValidator()
        {
            RuleFor(cartItem => cartItem.ProductItemId)
                .NotEmpty().WithMessage("ProductItemId is required.");

            RuleFor(cartItem => cartItem.ProductQuantity)
                .GreaterThan(0).WithMessage("ProductQuantity must be greater than 0.");
        }
    }
}
