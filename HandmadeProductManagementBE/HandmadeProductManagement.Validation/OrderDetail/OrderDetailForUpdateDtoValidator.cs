using FluentValidation;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.Validation.OrderDetail
{
    public class OrderDetailForUpdateDtoValidator : AbstractValidator<OrderDetailForUpdateDto>
    {
        public OrderDetailForUpdateDtoValidator()
        {
            RuleFor(x => x.ProductQuantity)
                .GreaterThan(0).WithMessage("Product quantity must be greater than 0.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than 0.");
        }
        
    }
}
