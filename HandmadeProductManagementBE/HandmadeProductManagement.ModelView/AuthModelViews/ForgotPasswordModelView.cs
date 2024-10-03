namespace HandmadeProductManagement.ModelViews.AuthModelViews;

public class ForgotPasswordModelView
{
    public string ClientUri { get; set; } = "https://localhost:7159/api/reset-password/";
    public required string Email { get; set; }
}