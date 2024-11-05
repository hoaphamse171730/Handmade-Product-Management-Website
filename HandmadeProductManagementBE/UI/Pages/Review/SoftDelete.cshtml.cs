using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Review
{
    public class SoftDeleteModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public SoftDeleteModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        [BindProperty]
        public string ReviewId { get; set; }

        public Task<IActionResult> OnGetAsync(string reviewId)
        {
            ReviewId = reviewId;
            return Task.FromResult<IActionResult>(Page());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var response = await _apiResponseHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/review/{ReviewId}/softdelete");

            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return RedirectToPage("Index");
            }

            ModelState.AddModelError(string.Empty, response.Message ?? "Error soft deleting the review.");
            return Page();
        }
    }
}
