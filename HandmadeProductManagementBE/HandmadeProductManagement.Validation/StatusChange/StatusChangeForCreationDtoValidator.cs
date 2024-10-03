
using FluentValidation;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;

namespace HandmadeProductManagement.Validation.StatusChange
{
    public class StatusChangeForCreationDtoValidator : AbstractValidator<StatusChangeForCreationDto>
    {
        public StatusChangeForCreationDtoValidator()
        {
            RuleFor(x => x.ChangeTime)
                .NotNull()
                .WithMessage("ChangeTime is required.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .WithMessage("Status is required.");
        }
    }
}
