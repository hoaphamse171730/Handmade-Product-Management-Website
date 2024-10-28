using Azure;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.UserModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.OpenApi.Models;

namespace UI.Pages.User
{
    public class DetailsModel : PageModel
    {

        private readonly ApiResponseHelper _apiResponseHelper;

        public DetailsModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public UserResponseByIdModel User;

        public async Task<IActionResult> OnGet(string userId)
        {
            var response = await _apiResponseHelper.GetAsync<UserResponseByIdModel>($"{Constants.ApiBaseUrl}/api/users/{userId}");
            if(response.StatusCode == StatusCodeHelper.OK)
            {
                User = response.Data;
            }

            return Page();
        }
    }
}
