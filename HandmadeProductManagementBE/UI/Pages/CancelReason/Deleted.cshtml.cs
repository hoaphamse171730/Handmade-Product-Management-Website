using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.CancelReasons
{
    public class DeletedModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public DeletedModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public List<CancelReasonDeletedDto> CancelReasons { get; set; } = new List<CancelReasonDeletedDto>();
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {

                // Get the deleted cancel reasons instead of categories
                var response = await _apiHelper.GetAsync<List<CancelReasonDeletedDto>>(
                    $"{Constants.ApiBaseUrl}/api/cancelreason/deleted");

                if (response != null && response.Data != null)
                {
                    CancelReasons = response.Data;
                }
                else
                {
                    CancelReasons = new List<CancelReasonDeletedDto>();
                }
            }
            catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
                if (ErrorMessage == "unauthorized") return RedirectToPage("/Login");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }
            return Page();
        }
    }
}
