using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Review
{
    public class DetailsModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public DetailsModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public ReviewModel Review { get; set; }

        public async Task<IActionResult> OnGetAsync(string reviewId)
        {
            var response = await _apiResponseHelper.GetAsync<ReviewModel>($"{Constants.ApiBaseUrl}/api/review/{reviewId}");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Review = response.Data;
            }

            return Page();
        }
    }
}
