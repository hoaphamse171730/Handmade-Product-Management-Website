using FluentValidation;
using HandmadeProductManagement.ModelViews.CartItemModelViews;

namespace HandmadeProductManagement.Validation.CartItem
{
    public class CartItemForUpdateDtoValidator : AbstractValidator<CartItemForUpdateDto>
    {
        public CartItemForUpdateDtoValidator()
        {
            RuleFor(x => x)
                .Must(dto => dto.ProductQuantity >= 0)
                .WithMessage("You must provide at least one valid field for update.");

            RuleFor(cartItem => cartItem.ProductQuantity)
                .GreaterThan(0).WithMessage("ProductQuantity must be greater than 0.")
                .When(cartItem => cartItem.ProductQuantity.HasValue);
        }
    }
}
