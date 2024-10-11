using FluentValidation;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace HandmadeProductManagement.Validation.CancelReason
{
    public class CancelReasonForUpdateDtoValidator : AbstractValidator<CancelReasonForUpdateDto>
    {
        public CancelReasonForUpdateDtoValidator() 
        {
            RuleFor(x => x)
                .Must(dto => !string.IsNullOrWhiteSpace(dto.Description) || dto.RefundRate >= 0)
                .WithMessage("You must provide at least one valid field for update.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description can not exceed 500 characters!")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Description can only contain letters and spaces!");
            RuleFor(x => x.RefundRate)
                .InclusiveBetween(0, 1);
        }
    }
}
