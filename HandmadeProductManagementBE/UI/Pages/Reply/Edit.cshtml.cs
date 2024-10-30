using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Reply
{
    public class EditModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public EditModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        [BindProperty]
        public ReplyModel Reply { get; set; }

        public async Task<IActionResult> OnGetAsync(string replyId)
        {
            var response = await _apiResponseHelper.GetAsync<ReplyModel>($"{Constants.ApiBaseUrl}/api/reply/{replyId}");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Reply = response.Data;
                return Page();
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostAsync(string replyId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var response = await _apiResponseHelper.PutAsync<BaseResponse<bool>>($"{Constants.ApiBaseUrl}/api/reply/{replyId}", new { content = Reply.Content });

            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return RedirectToPage("./Index");
            }

            ModelState.AddModelError(string.Empty, "Error updating reply");
            return Page();
        }
    }
}
