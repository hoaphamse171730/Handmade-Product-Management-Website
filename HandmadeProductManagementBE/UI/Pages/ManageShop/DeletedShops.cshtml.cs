using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Core.Common;
using System.Net;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ShopModelViews;

namespace UI.Pages.ManageShop
{
    public class DeletedShopModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public DeletedShopModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
        }

        public List<ShopDto> DeletedShops { get; set; } = new List<ShopDto>();
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 2;
        public bool HasNextPage { get; set; } = true;

        public async Task OnGetAsync(int pageNumber = 1, int pageSize = 2)
        {
            try
            {
                PageNumber = pageNumber;
                PageSize = pageSize;

                var response = await _apiHelper.GetAsync<List<ShopDto>>($"{Constants.ApiBaseUrl}/api/shop/deleted?pageNumber={PageNumber}&pageSize={PageSize}");
                if (response != null && response.Data != null)
                {
                    DeletedShops = response.Data;
                    HasNextPage = DeletedShops.Count == PageSize;
                }
                else
                {
                    DeletedShops = new List<ShopDto>();
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

        }

        [BindProperty]
        public string ShopIdToRecover { get; set; }

        public async Task<IActionResult> OnPostRecoverAsync()
        {
            try
            {
                var url = $"{Constants.ApiBaseUrl}/api/shop/{ShopIdToRecover}/recover";
                var response = await _apiHelper.PutAsync<bool>(url);

                if (response == null || response.StatusCode != StatusCodeHelper.OK)
                {
                    ModelState.AddModelError(string.Empty, "Unable to activate the shop. Please try again.");
                }

                return RedirectToPage("/ManageShop/DeletedShops");
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
