using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Seller
{
    public class OrderManagementModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public OrderManagementModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<OrderByUserDto>? Orders { get; set; }

        public async Task OnGetAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<OrderByUserDto>>(Constants.ApiBaseUrl + "/api/order/seller");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                Orders = response.Data.OrderByDescending(o => o.OrderDate).ToList();
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching orders.");
            }
        }
    }
}
