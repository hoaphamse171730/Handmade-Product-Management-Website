using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
    }
}
