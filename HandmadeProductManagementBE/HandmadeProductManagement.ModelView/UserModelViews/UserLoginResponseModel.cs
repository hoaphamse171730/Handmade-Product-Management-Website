namespace HandmadeProductManagement.ModelViews.UserModelViews;

public class UserLoginResponseModel
{
    public string? UserName { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string FullName { get; set; } = default!;
    public string Token { get; set; } = default!;
}