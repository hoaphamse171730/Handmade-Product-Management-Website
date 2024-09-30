namespace HandmadeProductManagement.ModelViews.AuthModelViews;

public class ResetPasswordModelView
{
    public required string Token { get; set; }

    private string _email = string.Empty;

    public required string Email
    {
        get => _email;
        set => _email = value.Trim();
    }

    public required string NewPassword { get; set; }
}