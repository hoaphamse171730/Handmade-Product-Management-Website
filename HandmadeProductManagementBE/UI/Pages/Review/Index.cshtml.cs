using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace UI.Pages.Review
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public IndexModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public IList<ReviewModel> Reviews { get; set; } = new List<ReviewModel>();
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }
        public IList<UserResponseModel> Users { get; set; } = new List<UserResponseModel>();
        public IList<ShopResponseModel> Shops { get; set; } = new List<ShopResponseModel>();

        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            PageNumber = pageNumber;

            var response = await _apiResponseHelper.GetAsync<IList<ReviewModel>>($"{Constants.ApiBaseUrl}/api/review?pageNumber={pageNumber}&pageSize={pageSize}");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Reviews = response.Data;
                var totalPagesResponse = await _apiResponseHelper.GetAsync<int>($"{Constants.ApiBaseUrl}/api/review/totalpages?pageSize={pageSize}");
                if (totalPagesResponse.StatusCode == StatusCodeHelper.OK)
                {
                    TotalPages = totalPagesResponse.Data;
                }
            }

            // Fetch all Users
            var userResponse = await _apiResponseHelper.GetAsync<IList<UserResponseModel>>($"{Constants.ApiBaseUrl}/api/users");
            if (userResponse.StatusCode == StatusCodeHelper.OK)
            {
                Users = userResponse.Data ?? new List<UserResponseModel>(); // Fallback to empty list if null
            }

            // Fetch all Shops
            var shopResponse = await _apiResponseHelper.GetAsync<IList<ShopResponseModel>>($"{Constants.ApiBaseUrl}/api/shop/get-all");
            if (shopResponse.StatusCode == StatusCodeHelper.OK)
            {
                Shops = shopResponse.Data ?? new List<ShopResponseModel>();  // Fallback to empty list if null;
            }

            return Page();
        }
    }
}
