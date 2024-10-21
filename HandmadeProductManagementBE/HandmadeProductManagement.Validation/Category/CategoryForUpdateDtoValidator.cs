using FluentValidation;
using HandmadeProductManagement.ModelViews.CategoryModelViews;

namespace HandmadeProductManagement.Validation.Category
{
    public class CategoryForUpdateDtoValidator : AbstractValidator<CategoryForUpdateDto>
    {
        public CategoryForUpdateDtoValidator()
        {
            RuleFor(category => category.Name)
<<<<<<< HEAD
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
                .When(x => !string.IsNullOrEmpty(x.Name));
=======
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
>>>>>>> 0ec24f9fc345fbc66919fc79c957a82dfd21f448

            RuleFor(category => category.Description)
                .MaximumLength(500).WithMessage("Category description must not exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));


        }
    }
}
