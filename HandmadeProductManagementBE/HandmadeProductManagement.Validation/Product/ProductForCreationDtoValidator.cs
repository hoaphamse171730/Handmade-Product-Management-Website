using FluentValidation;
using HandmadeProductManagement.ModelViews.ProductModelViews;

namespace HandmadeProductManagement.Validation.Product
{
    public class ProductForCreationDtoValidator : AbstractValidator<ProductForCreationDto>
    {
        public ProductForCreationDtoValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required!")
                .MaximumLength(500).WithMessage("Description can not exceed 500 characters!");
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required!")
                .MaximumLength(255).WithMessage("Name can not exceed 255 characters!");
        }
    }
}
