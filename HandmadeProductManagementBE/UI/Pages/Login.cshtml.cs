using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace UI.Pages;

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

    public IActionResult OnPostLogout()
    {
        var token = HttpContext.Session.GetString("Token");
        if (token != null)
        {
            HttpContext.Session.Remove("Token");
        }

        return RedirectToPage("/Login");
    }

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
        var response = await client.PostAsJsonAsync($"{Constants.ApiBaseUrl}/api/authentication/login", loginData);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
            var token = loginResponse?.Data?.Token;
            var userName = loginResponse?.Data?.UserName ?? string.Empty;

            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("Token", token);
                HttpContext.Session.SetString("UserName", userName);
                return Redirect("/");
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
            if (errorContent.Contains("The password you entered is incorrect"))
            {
                ModelState.AddModelError("Password", "The password you entered is incorrect.");
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                ErrorMessage = errorResponse?.Detail ?? "Login failed. Please try again.";
            }
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