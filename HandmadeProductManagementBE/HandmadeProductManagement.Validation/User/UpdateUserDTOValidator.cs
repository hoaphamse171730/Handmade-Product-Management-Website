using FluentValidation;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.ModelViews.UserModelViews;
namespace HandmadeProductManagement.Validation.User
{
    public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>    {

        public UpdateUserDTOValidator() {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
                
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\d{10}$").WithMessage("Invalid phone number")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.UserName)
                .Matches(CustomRegex.UsernameRegex).WithMessage("Invalid Username")
                .When(x => !string.IsNullOrWhiteSpace(x.UserName));


        }
    }
}
