using FluentValidation;
using HandmadeProductManagement.ModelViews.ProductModelViews;

namespace HandmadeProductManagement.Validation.Product
{
    public class ProductForUpdateDtoValidator : AbstractValidator<ProductForUpdateDto>
    {
        public ProductForUpdateDtoValidator()
        {
            RuleFor(x => x)
                .Must(dto => !string.IsNullOrWhiteSpace(dto.Name) ||
                             !string.IsNullOrWhiteSpace(dto.Description) ||
                             !string.IsNullOrWhiteSpace(dto.CategoryId))
                .WithMessage("At least one field (Name, Description, or CategoryId) must be provided for update.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters!");

            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Name cannot exceed 255 characters!")
                .Matches(@"^[a-zA-Z0-9\s]*$").WithMessage("Name can only contain letters, numbers, and spaces.");

            RuleFor(x => x.CategoryId)
                .Length(36).WithMessage("CategoryId must be 36 characters long (valid GUID format)."); 
        }
    }
}
