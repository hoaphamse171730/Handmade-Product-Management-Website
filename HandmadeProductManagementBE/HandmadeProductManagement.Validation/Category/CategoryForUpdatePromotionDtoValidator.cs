using FluentValidation;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
namespace HandmadeProductManagement.Validation.Category
{
    public class CategoryForUpdatePromotionDtoValidator : AbstractValidator<CategoryForUpdatePromotion>
    {
        public CategoryForUpdatePromotionDtoValidator()
        {
            RuleFor(category => category.promotionId)
                .NotEmpty().WithMessage("Promotion ID is required.");
        }
    }
}
