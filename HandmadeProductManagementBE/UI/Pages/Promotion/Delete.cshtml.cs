using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Promotion
{
    public class DeleteModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public DeleteModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        [BindProperty]
        public PromotionDto Promotion { get; set; }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            try { 

                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var response = await _apiHelper.GetAsync<PromotionDto>($"{Constants.ApiBaseUrl}/api/promotions/{id}");
                if (response != null && response.Data != null)
                {
                    Promotion = response.Data;
                    return Page();
                }
                else
                {
                    return NotFound();
                }
                } catch (BaseException.ErrorException ex)
                    {
                        ErrorMessage = ex.ErrorDetail.ErrorCode;
                        ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
                } catch (Exception ex)
                {
                    ErrorMessage = "An unexpected error occurred.";
                }
                return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var response = await _apiHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/promotions/{id}");
                if (response != null && response.Data)
                {
                    TempData["SuccessMessage"] = "Promotion deleted successfully.";
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while deleting the promotion.");
                    return Page();
                }
            }
            catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred.";
            }
            return Page();
        }
    }
}
