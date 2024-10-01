using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string destMail, string clientUri);
    Task SendPasswordRecoveryEmailAsync(string destMail, string passwordResetLink);
}