using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Review
{
    public class CreateModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public CreateModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        [BindProperty]
        public new string Content { get; set; }
        [BindProperty]
        public int Rating { get; set; }
        [BindProperty]
        public string ProductId { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var response = await _apiResponseHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/review", new
            {
                Content,
                Rating,
                ProductId,
                OrderId = "dummy-order-id"
            });

            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return RedirectToPage("Index");
            }

            ModelState.AddModelError(string.Empty, response.Message ?? "Error creating review.");
            return Page();
        }
    }
}
