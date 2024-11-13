using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace UI.Pages;

public class RecoveryModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    [BindProperty]
    public InputModel Input { get; set; }

    public string? StatusMessage { get; set; }

    public RecoveryModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool AcceptTerms { get; set; }
    }

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        var token = HttpContext.Session.GetString("Token");

        // Nếu đã có token trong session, chuyển hướng người dùng đến trang chính
        if (!string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Index"); // Hoặc trang bạn muốn chuyển đến
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5041/api/authentication/forgot-password");
            var jsonContent = JsonSerializer.Serialize(new { email = Input.Email });
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                StatusMessage = "Recovery email sent successfully. Please check your inbox.";
            }
            else
            {
                // Handle error response and extract detail
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Set the error message to the detail from the error response
                ErrorMessage = errorResponse?.Detail ?? "An error occurred while creating the shop.";
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error sending recovery email. Please try again.");
            return Page();
        }
    }
}