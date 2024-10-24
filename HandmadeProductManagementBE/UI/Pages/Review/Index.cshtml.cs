using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
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

            return Page();
        }

        //private async Task<string> GetUserDisplayName(Guid userId)
        //{
        //    var response = await _apiResponseHelper.GetAsync<UserInfoDto>($"{Constants.ApiBaseUrl}/api/userinfo/{userId}");
        //    return response.StatusCode == StatusCodeHelper.OK ? response.Data?.FullName ?? "Unknown User" : "Unknown User";
        //}

        //private async Task<string> GetShopName(string shopId)
        //{
        //    if (string.IsNullOrWhiteSpace(shopId))
        //        return "Unknown Shop";

        //    var response = await _apiResponseHelper.GetAsync<ShopResponseModel>($"{Constants.ApiBaseUrl}/api/shop/{shopId}");
        //    return response.StatusCode == StatusCodeHelper.OK ? response.Data?.Name ?? "Unknown Shop" : "Unknown Shop";
        //}
    }
}
