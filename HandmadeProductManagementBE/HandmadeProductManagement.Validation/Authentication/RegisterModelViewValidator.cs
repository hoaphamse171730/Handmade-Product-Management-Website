using FluentValidation;
using HandmadeProductManagement.ModelViews.AuthModelViews;

namespace HandmadeProductManagement.Validation.Authentication;

public class RegisterModelViewValidator : AbstractValidator<RegisterModelView>
{
    public RegisterModelViewValidator()
    {
        RuleFor(src => src.UserName)
            .NotEmpty();

        RuleFor(src => src.FullName)
            .NotEmpty();
        
        RuleFor(src => src.Email)
            .EmailAddress()
            .WithMessage("Format of Email is invalid.")
            .NotEmpty();
        
        RuleFor(src => src.PhoneNumber)
            .Matches(@"^\+?[0-9]{10,12}$")
            .WithMessage("Format of Phone number is invalid.")
            .NotEmpty();
        
        RuleFor(src => src.Password)
            .NotEmpty()
            .Matches("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$")
            .WithMessage("Password must be complex.");
    }
}