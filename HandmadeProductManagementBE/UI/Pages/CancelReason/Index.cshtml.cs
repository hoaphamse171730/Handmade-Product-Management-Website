using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;  // Import the CancelReasonModelViews
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.CancelReason
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public IndexModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public List<CancelReasonDto> CancelReasons { get; set; } = new List<CancelReasonDto>(); // Change from CategoryDto
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        // Fetch Cancel Reasons with pagination
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Modify API endpoint to get cancel reasons
                var response = await _apiHelper.GetAsync<List<CancelReasonDto>>(
                    $"{Constants.ApiBaseUrl}/api/cancelreason");

                if (response != null && response.Data != null)
                {
                    CancelReasons = response.Data;
                }
                else
                {
                    CancelReasons = new List<CancelReasonDto>();
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

        // Create new Cancel Reason
        public async Task<JsonResult> OnPostCreateCancelReasonAsync([FromBody] CancelReasonForCreationDto cancelReasonForCreation)
        {
            try
            {
                var response = await _apiHelper.PostAsync<bool>($"{Constants.ApiBaseUrl}/api/cancelreason", cancelReasonForCreation);

                if (response != null && response.Data)
                {
                    return new JsonResult(new { success = true, message = "Cancel reason created successfully." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to create cancel reason." });
                }
            }
            catch (BaseException.ErrorException ex)
            {
                return new JsonResult(new { success = false, message = ex.ErrorDetail.ErrorMessage?.ToString() });
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An unexpected error occurred." });
            }
        }
    }
}
