
using FluentValidation;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Validation.CancelReason
{
    public class CancelReasonForUpdateDtoValidator : AbstractValidator<CancelReasonForUpdateDto>
    {
        public CancelReasonForUpdateDtoValidator() 
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required!")
                .MaximumLength(500).WithMessage("Description can not exceed 500 characters!");
            RuleFor(x => x.RefundRate)
                .ExclusiveBetween(0, 1);
        }
    }
}
