using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages;

public class RegistrationModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegistrationModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    [Required(ErrorMessage = "User Name is required.")]
    public string UserName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Full Name is required.")]
    public string FullName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{9,10}$", ErrorMessage = "Incorrect number format. Phone number must be 9 to 10 digits.")]
    public string PhoneNumber { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Password is required.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$", ErrorMessage = "Password must be at least 8 characters long and include a mix of uppercase, lowercase, and special characters.")]
    public string Password { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Confirm Password is required.")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    [BindProperty]
    public string? ClientUri { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Please correct the errors in the form.";
            return Page();
        }

        var registrationData = new
        {
            UserName = this.UserName,
            FullName = this.FullName,
            Email = this.Email,
            PhoneNumber = this.PhoneNumber,
            Password = this.Password,
            ClientUri = this.ClientUri
        };

        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("http://localhost:5041/api/registration", registrationData);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("/Login");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            ErrorMessage = $"Registration failed: {errorContent}";
            return Page();
        }
    }
}
