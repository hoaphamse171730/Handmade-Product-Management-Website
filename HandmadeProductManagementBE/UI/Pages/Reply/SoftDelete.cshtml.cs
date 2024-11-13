using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Reply
{
    public class SoftDeleteModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public SoftDeleteModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public async Task<IActionResult> OnPostAsync(string replyId)
        {
            var response = await _apiResponseHelper.DeleteAsync<BaseResponse<bool>>($"{Constants.ApiBaseUrl}/api/reply/{replyId}/soft-delete");

            if (response.StatusCode == StatusCodeHelper.OK)
            {
                return RedirectToPage("./Index");
            }

            return Page();
        }
    }
}
