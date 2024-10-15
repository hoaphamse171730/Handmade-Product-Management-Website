
using FluentValidation;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;

namespace HandmadeProductManagement.Validation.StatusChange
{
    public class StatusChangeForUpdateDtoValidator : AbstractValidator<StatusChangeForUpdateDto>
    {
        public StatusChangeForUpdateDtoValidator() 
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status is required.");
        }
    }
}
