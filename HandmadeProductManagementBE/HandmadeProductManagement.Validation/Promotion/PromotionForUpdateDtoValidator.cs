using FluentValidation;
using HandmadeProductManagement.ModelViews.PromotionModelViews;

namespace HandmadeProductManagement.Validation.Promotion
{
    public class PromotionForUpdateDtoValidator : AbstractValidator<PromotionForUpdateDto>
    {
        public PromotionForUpdateDtoValidator()
        {

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required!")
                .MaximumLength(500).WithMessage("Description can not exceed 500 characters!")
                .When(x => !string.IsNullOrEmpty(x.Description));
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required!")
                .MaximumLength(255).WithMessage("Name can not exceed 255 characters!")
                .When(x => !string.IsNullOrEmpty(x.Name));
            RuleFor(x => x.DiscountRate);

            RuleFor(x => x.DiscountRate)
                .InclusiveBetween(0m, 100m)  
                .WithMessage("Discount percentage must be a decimal between 0 and 100.")
                .When(x => x.DiscountRate.HasValue);

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("Start date must be less than end date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        }
    }
}
