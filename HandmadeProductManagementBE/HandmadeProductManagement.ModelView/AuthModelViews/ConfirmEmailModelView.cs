namespace HandmadeProductManagement.ModelViews.AuthModelViews;

public class ConfirmEmailModelView
{
    public required string Email { get; set; }
    public required string Token { get; set; }
}