using HandmadeProductManagement.Core.Common;
using System.Text.Json.Serialization;

namespace HandmadeProductManagement.ModelViews.AuthModelViews;

public class ForgotPasswordModelView
{
    [JsonIgnore]
    public string ClientUri { get; set; } = $"{Constants.FrontUrl}/ResetPassword/";
    public required string Email { get; set; }
}