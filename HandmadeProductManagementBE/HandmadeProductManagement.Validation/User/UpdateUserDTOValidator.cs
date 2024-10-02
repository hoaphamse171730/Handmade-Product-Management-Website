using FluentValidation;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.ModelViews.UserModelViews;
namespace HandmadeProductManagement.Validation.User
{
    public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>    {

        public UpdateUserDTOValidator() {
            RuleFor(x => x.Email).
                EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber).
                Matches(@"^\d{10}$").WithMessage("Invalid Phone number");
            RuleFor(x => x.UserName).
                Matches(CustomRegex.UsernameRegex).WithMessage("Invalid Username");

        }
    }
}
