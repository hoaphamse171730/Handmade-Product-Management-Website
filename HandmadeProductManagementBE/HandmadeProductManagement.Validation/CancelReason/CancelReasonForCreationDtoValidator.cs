using FluentValidation;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Validation.CancelReason
{
    public class CancelReasonForCreationDtoValidator : AbstractValidator<CancelReasonForCreationDto>
    {
        public CancelReasonForCreationDtoValidator() 
        { 
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required!")
                .MaximumLength(500).WithMessage("Description can not exceed 500 characters!")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Description can only contain letters and spaces!");
            RuleFor(x => x.RefundRate)
                .InclusiveBetween(0,1);
        }
    }
}
