using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CartItemModelViews;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Checkout
{
    public class IndexModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public IndexModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<CartItemGroupDto> CartItems { get; set; } = new List<CartItemGroupDto>();
        public UserInfoDto UserInfo { get; set; } = new UserInfoDto();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }

        public string Token { get; set; }

        public async Task OnGetAsync()
        {
            CartItems = await GetCartItemsAsync();
            UserInfo = await GetUserInfoAsync();
            Token = HttpContext.Session.GetString("Token");
            ViewData["Token"] = Token;
        }

        private async Task<List<CartItemGroupDto>> GetCartItemsAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<CartItemGroupDto>>(Constants.ApiBaseUrl + "/api/cartitem");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                var cartItems = response.Data;

                Subtotal = cartItems.Sum(group => group.CartItems.Sum(item => item.TotalPriceEachProduct));
                Total = Subtotal;

                return cartItems;
            }
            return new List<CartItemGroupDto>();
        }

        private async Task<UserInfoDto> GetUserInfoAsync()
        {
            var response = await _apiResponseHelper.GetAsync<UserInfoDto>(Constants.ApiBaseUrl + "/api/userinfo");

            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return response.Data;
            }
            return new UserInfoDto();
        }
    }
}
