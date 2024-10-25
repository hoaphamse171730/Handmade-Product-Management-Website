using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Order
{
    public class OrderDetailModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public OrderDetailModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public OrderWithDetailDto Order { get; set; }
        public async Task OnGetAsync(string orderId)
        {
            await FetchOrderDetailsAsync(orderId);
        }

        private async Task FetchOrderDetailsAsync(string orderId)
        {
            var response = await _apiResponseHelper.GetAsync<OrderWithDetailDto>($"{Constants.ApiBaseUrl}/api/Order/{orderId}");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Order = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching order details.");
            }
        }
    }
}