using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.UserModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace UI.Pages.UserInfo
{
    public class EditUserModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;
        private readonly IHttpClientFactory _httpClientFactory;


        public EditUserModel(ApiResponseHelper apiResponseHelper, IHttpClientFactory httpClientFactory)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
            _httpClientFactory = httpClientFactory;
        }


        public UserResponseByIdModel userInfo { get; set; }
        public void OnGet()
        {
            string token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                string userId = GetUserIdFromToken(token);
                if (!string.IsNullOrEmpty(userId))
                {
                    userInfo = GetUserResponseById(userId);
                }
            }
        }
        private string GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
            return userIdClaim?.Value;
        }
        private UserResponseByIdModel GetUserResponseById(string id)
        {
            var response = _apiResponseHelper.GetAsync<UserResponseByIdModel>(Constants.ApiBaseUrl + $"/api/users/{id}").Result;
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new UserResponseByIdModel();
        }
        [BindProperty]
        public UpdateUserDTO updateUser { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string token = HttpContext.Session.GetString("Token");
            string userId = GetUserIdFromToken(token);

            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{Constants.ApiBaseUrl}/api/users/{userId}", updateUser);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var baseResponse = JsonSerializer.Deserialize<BaseResponse<bool>>(content, options);

                if (baseResponse != null && baseResponse.StatusCode == StatusCodeHelper.OK)
                {
                    return RedirectToPage("/UserInfo/UserInfo");
                }

                ModelState.AddModelError(string.Empty, baseResponse?.Message ?? "Error updating user information.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating user information.");
            }

            return Page();
        }
    }
}

