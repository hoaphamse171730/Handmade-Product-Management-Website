using FluentValidation;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;

namespace HandmadeProductManagement.Validation.UserInfo
{
    public class UserInfoForUpdateDtoValidator : AbstractValidator<UserInfoForUpdateDto>
    {
        public UserInfoForUpdateDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(255).WithMessage("Full name cannot be longer than 255 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Full name can only contain letters and spaces.");
            RuleFor(x => x.DisplayName)
                .MaximumLength(100).WithMessage("Display name cannot be longer than 100 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio cannot be longer than 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Bio));
            RuleFor(x => x.BankAccount)
                .Matches(@"^\d+$").WithMessage("Bank account should contain only digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.BankAccount));
            RuleFor(x => x.BankAccountName)
                .MaximumLength(255).WithMessage("Bank account name cannot be longer than 255 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.BankAccountName));
            RuleFor(x => x.Bank)
                .MaximumLength(255).WithMessage("Bank name cannot be longer than 255 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Bank));
            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot be longer than 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Address));
            RuleFor(x => x.AvatarUrl)
                .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Avatar URL must be a valid URL.")
                .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));
        }
    }
}
