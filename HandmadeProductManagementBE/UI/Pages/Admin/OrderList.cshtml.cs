using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Pages.Order;

namespace UI.Pages.Admin
{
    public class OrderListModel : PageModel
    {
        private readonly ILogger<OrderHistoryModel> _logger;
        private readonly ApiResponseHelper _apiResponseHelper;

        public OrderListModel(ILogger<OrderHistoryModel> logger, ApiResponseHelper apiResponseHelper)
        {
            _logger = logger;
            _apiResponseHelper = apiResponseHelper;
        }

        public List<OrderByUserDto>? Orders { get; set; }
        public string CurrentFilter { get; set; } = "All";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool HasNextPage { get; set; } = true;

        public async Task OnGetAsync(string? filter, int pageNumber = 1, int pageSize = 12)
        {
            CurrentFilter = filter ?? "All";

            PageNumber = pageNumber;
            PageSize = pageSize;

            var response = await _apiResponseHelper.GetAsync<List<OrderByUserDto>>(Constants.ApiBaseUrl + $"/api/order?pageNumber={PageNumber}&pageSize={PageSize}");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                var orders = response.Data.OrderByDescending(o => o.OrderDate).ToList();
                Orders = CurrentFilter switch
                {
                    "Pending" => orders.Where(o => o.Status == "Pending").ToList(),
                    "Processing" => orders.Where(o => o.Status == "Processing").ToList(),
                    "Delivering" => orders.Where(o => new[] { "Delivery Failed", "Delivering", "On Hold", "Delivering Retry" }.Contains(o.Status)).ToList(),
                    "Shipped" => orders.Where(o => o.Status == "Shipped").ToList(),
                    "Canceled" => orders.Where(o => o.Status == "Canceled").ToList(),
                    "Refunded" => orders.Where(o => new[] { "Refund Requested", "Refund Denied", "Refund Approve", "Refunded" }.Contains(o.Status)).ToList(),
                    _ => orders
                };
                HasNextPage = Orders.Count == PageSize;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching orders.");
            }
        }
    }
}
