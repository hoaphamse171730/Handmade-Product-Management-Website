namespace HandmadeProductManagement.ModelViews.AuthModelViews;

public class ResetPasswordModelView
{
    public required string Email { get; set; }
    public required string NewPassword { get; set; }
    public required string Token { get; set; }
}
