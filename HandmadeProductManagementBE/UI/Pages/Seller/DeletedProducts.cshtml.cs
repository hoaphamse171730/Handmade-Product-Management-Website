using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Seller
{
    public class DeletedProductsModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public DeletedProductsModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        [BindProperty]
        public List<HandmadeProductManagement.Contract.Repositories.Entity.Product> DeletedProducts { get; set; } = new();

        [TempData]
        public string? ErrorMessage { get; set; }

        [TempData]
        public string? ErrorDetail { get; set; }
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var response = await _apiResponseHelper.GetAsync<List<HandmadeProductManagement.Contract.Repositories.Entity.Product>>(
                    $"{Constants.ApiBaseUrl}/api/product/all-deleted-products?pageNumber={pageNumber}&pageSize={pageSize}");

                if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
                {
                    DeletedProducts = response.Data;
                }
                else
                {
                    ErrorMessage = response.Message ?? "Failed to fetch deleted products";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred while fetching deleted products";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRecoverProductAsync([FromQuery] string productId)
        {
            try
            {
                var response = await _apiResponseHelper.PutAsync<bool>(
                    $"{Constants.ApiBaseUrl}/api/product/{productId}/recover");

                if (response.StatusCode == StatusCodeHelper.OK && response.Data)
                {
                    return new JsonResult(new { success = true });
                }

                return BadRequest(response.Message ?? "Failed to recover product");
            }
            catch (Exception ex)
            {
                return BadRequest("An unexpected error occurred while recovering the product");
            }
        }
    }
}
