using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagement.ModelViews.AuthModelViews
{
    public class LoginModelView
    {
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
