using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Store;
using GraphQLParser;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.Core.Common;
using System.Net.Http.Headers;
using System.Text.Json;

namespace UI.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public ChangePasswordModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        [Required(ErrorMessage = "Current password is required.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please confirm your new password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string Token { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors and try again.";
                return Page();
            }

            var changePasswordModel = new ChangePasswordModelView
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
            };

            Token = HttpContext.Session.GetString("Token");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            var response = await client.PostAsJsonAsync($"{Constants.ApiBaseUrl}/api/authentication/change-password", changePasswordModel);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Password changed successfully.";
                ModelState.Clear();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ErrorMessage = errorResponse?.Detail ?? "An error occurred while changing the password.";
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            return Page();
        }
    }
}
