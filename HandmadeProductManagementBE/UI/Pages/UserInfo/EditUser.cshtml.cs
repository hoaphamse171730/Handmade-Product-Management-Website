using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews; // Thêm namespace UserInfoModelViews
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
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
        public UserInfoDto userInfoDto { get; set; }

        [BindProperty]
        public UserInfoUpdateRequest UpdateRequest { get; set; } // Thêm UserInfoUpdateRequest để bind dữ liệu

        public void OnGet()
        {
            string token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                string userId = GetUserIdFromToken(token);
                if (!string.IsNullOrEmpty(userId))
                {
                    userInfo = GetUserResponseById(userId);
                    userInfoDto = GetUserInfoById(userId);
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

        private UserInfoDto GetUserInfoById(string id)
        {
            var response = _apiResponseHelper.GetAsync<UserInfoDto>(Constants.ApiBaseUrl + $"/api/userinfo").Result;
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new UserInfoDto();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string token = HttpContext.Session.GetString("Token");
            string userId = GetUserIdFromToken(token);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();
            if (UpdateRequest.AvtFile != null)
            {
                var fileContent = new StreamContent(UpdateRequest.AvtFile.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(UpdateRequest.AvtFile.ContentType);
                content.Add(fileContent, "AvtFile", UpdateRequest.AvtFile.FileName);
            }

            var jsonData = JsonSerializer.Serialize(UpdateRequest.UserInfo);
            content.Add(new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json"), "UserInfo");

            var response = await client.PutAsync($"{Constants.ApiBaseUrl}/api/users/{userId}", content);

            if (response.IsSuccessStatusCode)
            {
                var contentResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var baseResponse = JsonSerializer.Deserialize<BaseResponse<bool>>(contentResponse, options);

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
