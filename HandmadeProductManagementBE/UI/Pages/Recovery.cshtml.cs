using HandmadeProductManagement.Core.Common;
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

    public void OnGet()
    {
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
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{Constants.ApiBaseUrl}/api/authentication/forgot-password");
            var jsonContent = JsonSerializer.Serialize(new { email = Input.Email });
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                StatusMessage = "Recovery email sent successfully. Please check your inbox.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Error: {error}");
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