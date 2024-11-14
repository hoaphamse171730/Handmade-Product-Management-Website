using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using Microsoft.AspNetCore.Mvc;

namespace UI.Pages.Shared
{
    public class CartItemListViewComponent : ViewComponent
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public CartItemListViewComponent(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var (cartItem, totalCartItemCount) = await GetCartItemAsync();
            ViewData["CartItemCount"] = totalCartItemCount;
            return View(cartItem);
        }

        private async Task<(List<CartItemGroupDto>, int)> GetCartItemAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<CartItemGroupDto>>($"{Constants.ApiBaseUrl}/api/cartitem");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                // Calculate total cart item count across all groups
                var totalCartItemCount = response.Data.Sum(group => group.TotalCartItemCount);
                return (response.Data, totalCartItemCount);
            }

            return (new List<CartItemGroupDto>(), 0);
        }
    }
}
