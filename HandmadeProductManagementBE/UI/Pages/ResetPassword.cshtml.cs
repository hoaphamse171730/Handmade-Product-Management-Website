using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.Core.Common;
namespace UI.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public ResetPasswordModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        [BindProperty]
        public string Token { get; set; }

        public void OnGet()
        {
            // Logging the entire query string for debugging
            Console.WriteLine($"Query String: {Request.QueryString}");

            Email = Request.Query["email"];
            Token = Request.Query["token"];

            Console.WriteLine($"Email: {Email}, Token: {Token}");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors and try again.";
                return Page();
            }

            var resetPasswordModel = new ResetPasswordModelView
            {
                Email = Email,
                NewPassword = NewPassword,
                Token = Token
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{Constants.ApiBaseUrl}/api/authentication/reset-password", resetPasswordModel);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Password reset successfully.";
                ModelState.Clear();
                return RedirectToPage("/SuccessPages/ResetPasswordSuccess");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"An error occurred: {errorContent}");
            }

            return Page(); 
        }
    }
}
