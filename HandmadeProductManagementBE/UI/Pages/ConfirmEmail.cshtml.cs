using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace UI.Pages
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;


        public ConfirmEmailModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Token { get; set; } = string.Empty;

        public string? ConfirmationResult { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Email = Request.Query["email"];
            Token = Request.Query["token"];


            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Token))
            {
                ErrorMessage = "Invalid email confirmation request.";
                return Page();
            }

            var encodedToken = HttpUtility.UrlEncode(Token);


            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync($"{Constants.ApiBaseUrl}/api/authentication/confirm-email", new
            {
                Email,
                Token = encodedToken
            });

            if (response.IsSuccessStatusCode)
            {
                ConfirmationResult = "Email confirmed successfully!";
                return RedirectToPage("/Login");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error: {error}");
                ErrorMessage = "Failed to confirm email. Please try again later.";
                return RedirectToPage("/error");
            }
            }
    }
}
