using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Reply
{
    public class CreateModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public CreateModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        [BindProperty]
        public ReplyModel Reply { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await _apiResponseHelper.PostAsync<BaseResponse<bool>>($"{Constants.ApiBaseUrl}/api/reply", new { content = Reply.Content, reviewId = Reply.ReviewId });

            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Error creating reply");
            return Page();
        }
    }
}
