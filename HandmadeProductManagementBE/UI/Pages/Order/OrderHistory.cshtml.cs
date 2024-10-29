using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Order
{
    public class OrderHistoryModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public OrderHistoryModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<OrderByUserDto>? Orders { get; set; }
        public string CurrentFilter { get; set; } = "All";

        public async Task OnGetAsync(string? filter)
        {
            // Set default filter to "All" if none is provided
            CurrentFilter = filter ?? "All";

            var response = await _apiResponseHelper.GetAsync<List<OrderByUserDto>>(Constants.ApiBaseUrl + "/api/order/user");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                var orders = response.Data.OrderByDescending(o => o.OrderDate).ToList();

                // Filter orders based on selected filter
                Orders = CurrentFilter switch
                {
                    "Pending" => orders.Where(o => o.Status == "Pending").ToList(),
                    "AwaitingPayment" => orders.Where(o => o.Status == "Awaiting Payment").ToList(),
                    "Processing" => orders.Where(o => o.Status == "Processing").ToList(),
                    "Delivering" => orders.Where(o => new[] { "Delivery Failed", "Delivering", "On Hold", "Delivering Retry" }.Contains(o.Status)).ToList(),
                    "Shipped" => orders.Where(o => o.Status == "Shipped").ToList(),
                    "Canceled" => orders.Where(o => o.Status == "Canceled").ToList(),
                    "Refunded" => orders.Where(o => new[] { "Refund Requested", "Refund Denied", "Refund Approve", "Refunded" }.Contains(o.Status)).ToList(),
                    _ => orders // Default to show all orders
                };
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching orders.");
            }
        }
    }
}
