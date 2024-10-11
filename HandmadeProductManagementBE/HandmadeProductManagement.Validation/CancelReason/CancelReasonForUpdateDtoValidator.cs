using FluentValidation;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Validation.CancelReason
{
    public class CancelReasonForUpdateDtoValidator : AbstractValidator<CancelReasonForUpdateDto>
    {
        public CancelReasonForUpdateDtoValidator() 
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description can not exceed 500 characters!");
            RuleFor(x => x.RefundRate)
                .InclusiveBetween(0, 1);
        }
    }
}
