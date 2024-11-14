using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace UI.Pages;

public class RegistrationModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ApiResponseHelper _apiResponseHelper;

    public RegistrationModel(IHttpClientFactory httpClientFactory, ApiResponseHelper apiResponseHelper)
    {
        _httpClientFactory = httpClientFactory;
        _apiResponseHelper = apiResponseHelper;
    }

    [BindProperty]
    [Required(ErrorMessage = "User Name is required.")]
    [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "User Name cannot contain spaces or special characters.")]
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
        };
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync($"{Constants.ApiBaseUrl}/api/authentication/register", registrationData);
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Error Details: " + errorContent);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage("/Login");
        }
        else
        {

            if (errorContent.Contains(Constants.ErrorMessageEmailTaken))
            {
                ModelState.AddModelError("Email", Constants.ErrorMessageEmailTaken);
            }
            if (errorContent.Contains(Constants.ErrorMessagePhoneTaken))
            {
                ModelState.AddModelError("PhoneNumber", Constants.ErrorMessagePhoneTaken);
            }
            if (errorContent.Contains(Constants.ErrorMessageUsernameTaken))
            {
                ModelState.AddModelError("UserName", Constants.ErrorMessageUsernameTaken);
            }
            ErrorMessage = "Registration failed. Please correct the errors and try again.";
            return Page();
        }
    }
}