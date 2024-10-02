namespace HandmadeProductManagement.ModelViews.AuthModelViews
{
    public class LoginModelView
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public required string Password { get; set; }
    }
}
