namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IEmailService
{
    Task SendPasswordRecoveryEmailAsync(string destMail, string passwordResetLink);
}