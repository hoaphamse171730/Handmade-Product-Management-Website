using FluentValidation;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.Validation.OrderDetail
{
    public class OrderDetailForCreationDtoValidator : AbstractValidator<OrderDetailForCreationDto>
    {
        public OrderDetailForCreationDtoValidator()
        {
            RuleFor(x => x.ProductItemId)
                .NotNull().WithMessage("Product ID must not be null.");

            RuleFor(x => x.OrderId)
                .NotNull().WithMessage("Order ID must not be null.");

            RuleFor(x => x.ProductQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Product quantity must be greater than or equal 0.");

            RuleFor(x => x.DiscountPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal 0.");
        }
    }
}