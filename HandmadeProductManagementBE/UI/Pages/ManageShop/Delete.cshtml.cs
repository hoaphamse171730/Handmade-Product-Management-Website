using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.ManageShop
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
        public ShopDto Shop { get; set; } = new ShopDto();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            try { 

                if (string.IsNullOrEmpty(id))
                {
                    return NotFound();
                }

                var response = await _apiHelper.GetAsync<ShopDto>($"{Constants.ApiBaseUrl}/api/shop/admin_get/{id}");
                if (response != null && response.Data != null)
                {
                    Shop = response.Data;
                    return Page();
                }
                else
                {
                    return NotFound();
                }
            } 
            catch (BaseException.ErrorException ex)
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

                var response = await _apiHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/shop/deleteById/{id}");
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
