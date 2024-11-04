using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using HandmadeProductManagement.ModelViews.CancelReasonModelViews;

namespace UI.Pages.Order
{
    public class OrderHistoryModel : PageModel
    {
        private readonly ILogger<OrderHistoryModel> _logger;
        private readonly ApiResponseHelper _apiResponseHelper;

        public OrderHistoryModel(ILogger<OrderHistoryModel> logger, ApiResponseHelper apiResponseHelper)
        {
            _logger = logger;
            _apiResponseHelper = apiResponseHelper;
        }

        public List<OrderByUserDto>? Orders { get; set; }
        public string CurrentFilter { get; set; } = "All";

        public async Task OnGetAsync(string? filter)
        {
            CurrentFilter = filter ?? "All";

            var response = await _apiResponseHelper.GetAsync<List<OrderByUserDto>>(Constants.ApiBaseUrl + "/api/order/user");

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
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching orders.");
            }
        }

        public async Task<IActionResult> OnPatchCancelOrderAsync(string orderId)
        {
            // Call GetByDescription to get the CancelReasonId
            var cancelReasonResponse = await _apiResponseHelper.GetAsync<CancelReasonDto>(Constants.ApiBaseUrl + $"/api/cancelreason/description/{Constants.CustomerCancelReason}");

            if (cancelReasonResponse?.StatusCode != StatusCodeHelper.OK || cancelReasonResponse.Data == null)
            {
                return new JsonResult(new { success = false, message = cancelReasonResponse?.Message ?? "An error occurred while fetching the cancel reason." });
            }

            var updateStatusDto = new UpdateStatusOrderDto
            {
                OrderId = orderId,
                Status = "Canceled",
                CancelReasonId = cancelReasonResponse.Data.Id
            };

            var response = await _apiResponseHelper.PatchAsync<bool>(Constants.ApiBaseUrl + "/api/order/status", updateStatusDto);

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data)
            {
                // Refresh the orders list after successful cancellation
                await OnGetAsync(CurrentFilter);
                return new JsonResult(new { success = true });
            }
            else
            {
                return new JsonResult(new { success = false, message = response?.Message ?? "An error occurred while canceling the order." });
            }
        }

    }
}
