using HandmadeProductManagement.Core.Store;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using Microsoft.AspNetCore.Mvc;
namespace UI.Pages.ManageShop
{
    public class ShopModel : PageModel
    {
        private readonly ApiResponseHelper _apiHelper;

        public ShopModel(ApiResponseHelper apiHelper)
        {
            _apiHelper = apiHelper;
            Shops = new List<ShopDto>();
        }

        public List<ShopDto> Shops { get; set; }
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

                var response = await _apiHelper.GetAsync<List<ShopDto>>($"{Constants.ApiBaseUrl}/api/shop/admin?pageNumber={PageNumber}&pageSize={PageSize}");
                if (response != null && response.Data != null)
                {
                    Shops = response.Data;
                    HasNextPage = Shops.Count == PageSize;
                }
                else
                {
                    Shops = new List<ShopDto>();
                }
            } catch (BaseException.ErrorException ex)
            {
                ErrorMessage = ex.ErrorDetail.ErrorCode;
                ErrorDetail = ex.ErrorDetail.ErrorMessage?.ToString();
            }
            catch (Exception ex)
                {
                    ErrorMessage = "An unexpected error occurred.";
                }
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
            {
                try
                {
                    var response = await _apiHelper.DeleteAsync<bool>($"{Constants.ApiBaseUrl}/api/shop/deleteById/{id}");
                    if (response != null && response.Data)
                    {
                        // Refresh the shops list after deletion
                        await OnGetAsync(PageNumber, PageSize);
                        return Page();
                    }
                    else
                    {
                        ErrorMessage = "Failed to delete the shop.";
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
