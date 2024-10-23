using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Review
{
    public class EditModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public EditModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        [BindProperty]
        public new string Content { get; set; }
        [BindProperty]
        public int Rating { get; set; }
        [BindProperty]
        public string ReviewId { get; set; }

        public async Task<IActionResult> OnGetAsync(string reviewId)
        {
            ReviewId = reviewId;
            var response = await _apiResponseHelper.GetAsync<ReviewModel>($"{Constants.ApiBaseUrl}/api/review/{reviewId}");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Content = response.Data.Content ?? string.Empty;
                Rating = response.Data.Rating;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var response = await _apiResponseHelper.PutAsync<ReviewModel>(
                $"{Constants.ApiBaseUrl}/api/review/{ReviewId}",
                new { Content, Rating }
            );

            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return RedirectToPage("Index");
            }

            ModelState.AddModelError(string.Empty, response.Message ?? "Error updating review.");
            return Page();
        }
    }
}
