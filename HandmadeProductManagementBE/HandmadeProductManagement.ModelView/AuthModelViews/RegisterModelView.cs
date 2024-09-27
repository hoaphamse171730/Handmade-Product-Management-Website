using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagement.ModelViews.AuthModelViews;

public class RegisterModelView
{
    [Required] public string UserName { get; set; } = default!;
    [Required] public string FullName { get; set; } = default!;
    [Required] [EmailAddress] public string Email { get; set; } = default!;
    [Required] [Phone] public string PhoneNumber { get; set; } = default!;

    [Required]
    [RegularExpression("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$",
        ErrorMessage = "Password must be complex")]
    public string Password { get; set; } = default!;

}