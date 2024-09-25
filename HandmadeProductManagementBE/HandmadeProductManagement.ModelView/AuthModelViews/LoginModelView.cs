using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagement.ModelViews.AuthModelViews
{
    public class LoginModelView
    {
        [Required] public required string Username { get; set; }


        [Required] public required string Password { get; set; }

    }
}
