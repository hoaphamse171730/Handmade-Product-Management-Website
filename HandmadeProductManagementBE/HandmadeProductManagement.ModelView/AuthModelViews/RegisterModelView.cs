using HandmadeProductManagement.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagement.ModelViews.AuthModelViews;

public class RegisterModelView
{
    public string UserName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    
    public string Password { get; set; } = default!;

    public string ClientUri { get; set; } = $"{Constants.FrontUrl}/ConfirmEmail";

}