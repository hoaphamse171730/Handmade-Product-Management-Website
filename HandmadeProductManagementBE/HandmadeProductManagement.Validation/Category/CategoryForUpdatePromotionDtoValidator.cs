using FluentValidation;
using HandmadeProductManagement.ModelViews.CategoryModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Validation.Category
{
    public class CategoryForUpdatePromotionDtoValidator : AbstractValidator<CategoryForUpdatePromotion>
    {
        public CategoryForUpdatePromotionDtoValidator()
        {
            RuleFor(category => category.promotionId)  // Đảm bảo sử dụng đúng tên thuộc tính
                .NotEmpty().WithMessage("Promotion ID is required.")
                .Matches(@"^[{]?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}[}]?$")
                .WithMessage("Promotion ID must be in a valid GUID format.");
        }
    }
}
