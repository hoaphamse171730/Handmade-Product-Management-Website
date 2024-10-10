using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagement.Validation.Promotion
{
    public class PromotionForCreationDtoValidator : AbstractValidator<PromotionForCreationDto>
    {
        public PromotionForCreationDtoValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required!")
                .MaximumLength(500).WithMessage("Description can not exceed 500 characters!");  
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required!")
                .MaximumLength(255).WithMessage("Name can not exceed 255 characters!");
            RuleFor(x => x.DiscountRate);

            RuleFor(x => x.DiscountRate)
                .InclusiveBetween(0m, 100m)  
                .WithMessage("Discount percentage must be a float between 0 and 100.");
            
            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date must be less than or equal to end date.");

        }

    }
}
