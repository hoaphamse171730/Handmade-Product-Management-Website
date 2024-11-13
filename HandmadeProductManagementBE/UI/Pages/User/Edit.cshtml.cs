using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.UserModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace UI.Pages.User
{
    public class EditModel : PageModel
    {
        private readonly ApiResponseHelper _apiResposeHelper;
        private readonly IHttpClientFactory _httpClientFactory;

        public EditModel(ApiResponseHelper apiResposeHelper, IHttpClientFactory httpClientFactory) {
            _apiResposeHelper = apiResposeHelper;
            _httpClientFactory = httpClientFactory;
        }

        public UserResponseByIdModel userInfo { get; set; }
        public async Task<IActionResult> OnGet(string userId)
        {
            var response = await _apiResposeHelper.GetAsync<UserResponseByIdModel>(Constants.ApiBaseUrl + $"/api/users/{userId}");
            if (response != null) {
                userInfo = response.Data;
            }
            return Page();
        }

        [BindProperty]
        public UpdateUserDTO updateUser {  get; set; }

        public async Task<IActionResult> OnPostAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PutAsJsonAsync($"{Constants.ApiBaseUrl}/api/users/{userId}", updateUser);

            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                var baseResponse = JsonSerializer.Deserialize<BaseResponse<bool>>(content, options);
                if(baseResponse != null && baseResponse.StatusCode == StatusCodeHelper.OK) {
                    return RedirectToPage("/User/User");
                }
                ModelState.AddModelError(string.Empty, baseResponse?.Message ?? "Error updating user info");
            }
            else {
                ModelState.AddModelError(string.Empty, "An error occurred while updating user information.");
            }
            return Page();
        }


    }
}
