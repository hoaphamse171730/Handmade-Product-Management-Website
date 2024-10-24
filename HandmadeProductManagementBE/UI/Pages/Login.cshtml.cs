using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

public class LoginModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [BindProperty]
    public string Password { get; set; }

    private readonly IHttpClientFactory _httpClientFactory;

    public LoginModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Invalid login attempt.";
            return Page();
        }

        var loginData = new
        {
            Email = this.Email,
            Password = this.Password
        };
        var client = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("http://localhost:5041/api/authentication/login", loginData);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
            var token = loginResponse?.Data?.Token;
            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("Token", token);
                return RedirectToPage("/HomePage");
            }
            else
            {
                ErrorMessage = "Login failed. Token not found.";
                return Page();
            }
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            ErrorMessage = errorResponse?.Detail ?? "Login failed. Please try again.";
            return Page();
        }
    }
}

public class LoginResponse
{
    public LoginData? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
    public string? Code { get; set; }
}

public class LoginData
{
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public string? FullName { get; set; }
    public string? Token { get; set; }
}

public class ErrorResponse
{
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public string? Instance { get; set; }
    public string? TraceId { get; set; }
}
