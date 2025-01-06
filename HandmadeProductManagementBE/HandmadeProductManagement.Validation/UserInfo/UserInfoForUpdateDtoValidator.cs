using FluentValidation;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;

namespace HandmadeProductManagement.Validation.UserInfo
{
    public class UserInfoForUpdateDtoValidator : AbstractValidator<UserInfoForUpdateDto>
    {
        public UserInfoForUpdateDtoValidator()
        {
            // Validate Bio (Max 500 characters)
            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio cannot be longer than 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Bio));

            // Validate Bank Account (Only digits)
            RuleFor(x => x.BankAccount)
                .Matches(@"^\d+$").WithMessage("Bank account should contain only digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.BankAccount));

            // Validate Bank Account Name (Max 255 characters, only letters and spaces)
            RuleFor(x => x.BankAccountName)
                .MaximumLength(255).WithMessage("Bank account name cannot be longer than 255 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Bank account name can only contain letters and spaces.")
                .When(x => !string.IsNullOrWhiteSpace(x.BankAccountName));

            // Validate Bank (Max 255 characters, only letters and spaces)
            RuleFor(x => x.Bank)
                .MaximumLength(255).WithMessage("Bank name cannot be longer than 255 characters.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Bank name can only contain letters and spaces.")
                .When(x => !string.IsNullOrWhiteSpace(x.Bank));

            // Optional Avatar URL validation (if needed)
            // RuleFor(x => x.AvatarUrl)
            //     .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Avatar URL must be a valid URL.")
            //     .When(x => !string.IsNullOrWhiteSpace(x.AvatarUrl));
        }
    }
}
