namespace HandmadeProductManagement.ModelViews.AuthModelViews
{
    public class ChangePasswordModelView
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
