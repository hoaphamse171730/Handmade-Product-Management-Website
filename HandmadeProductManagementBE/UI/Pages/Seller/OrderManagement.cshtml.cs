using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.OrderModelViews;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Contract.Repositories.Entity;
using System.Net.Http;

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

        public List<CancelReason> CancelReasons { get; set; } = new List<CancelReason>();

        public string CurrentFilter { get; set; } = "All";

        // Define valid status transitions
        private readonly Dictionary<string, List<string>> validStatusTransitions = new Dictionary<string, List<string>>
        {
            { Constants.OrderStatusPending, new List<string> { Constants.OrderStatusCanceled, Constants.OrderStatusAwaitingPayment } },
            { Constants.OrderStatusAwaitingPayment, new List<string> { Constants.OrderStatusCanceled, Constants.OrderStatusProcessing } },
            { Constants.OrderStatusProcessing, new List<string> { Constants.OrderStatusDelivering } },
            { Constants.OrderStatusDelivering, new List<string> { Constants.OrderStatusShipped, Constants.OrderStatusDeliveryFailed } },
            { Constants.OrderStatusDeliveryFailed, new List<string> { Constants.OrderStatusOnHold } },
            { Constants.OrderStatusOnHold, new List<string> { Constants.OrderStatusDeliveringRetry, Constants.OrderStatusRefundRequested, Constants.OrderStatusReturning } },
            { Constants.OrderStatusRefundRequested, new List<string> { Constants.OrderStatusRefundDenied, Constants.OrderStatusRefundApprove } },
            { Constants.OrderStatusRefundApprove, new List<string> { Constants.OrderStatusReturning } },
            { Constants.OrderStatusReturning, new List<string> { Constants.OrderStatusReturnFailed, Constants.OrderStatusReturned } },
            { Constants.OrderStatusReturnFailed, new List<string> { Constants.OrderStatusOnHold } },
            { Constants.OrderStatusReturned, new List<string> { Constants.OrderStatusRefunded } },
            { Constants.OrderStatusRefunded, new List<string> { Constants.OrderStatusClosed } },
            { Constants.OrderStatusCanceled, new List<string> { Constants.OrderStatusClosed } },
            { Constants.OrderStatusDeliveringRetry, new List<string> { Constants.OrderStatusDelivering } },
        };

        public async Task OnGetAsync(string? filter)
        {
            // Set default filter to "All" if none is provided
            CurrentFilter = filter ?? "All";

            var response = await _apiResponseHelper.GetAsync<List<OrderByUserDto>>(Constants.ApiBaseUrl + "/api/order/seller");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                var orders = response.Data.OrderByDescending(o => o.OrderDate).ToList();

                // Filter orders based on selected filter
                Orders = CurrentFilter switch
                {
                    "Pending" => orders.Where(o => o.Status == Constants.OrderStatusPending).ToList(),
                    "AwaitingPayment" => orders.Where(o => o.Status == Constants.OrderStatusAwaitingPayment).ToList(),
                    "Processing" => orders.Where(o => o.Status == Constants.OrderStatusProcessing).ToList(),
                    "Delivering" => orders.Where(o => new[] { Constants.OrderStatusDeliveryFailed, Constants.OrderStatusDelivering, Constants.OrderStatusOnHold, Constants.OrderStatusDeliveringRetry }.Contains(o.Status)).ToList(),
                    "Shipped" => orders.Where(o => o.Status == Constants.OrderStatusShipped).ToList(),
                    "Canceled" => orders.Where(o => o.Status == Constants.OrderStatusCanceled).ToList(),
                    "Refunded" => orders.Where(o => new[] { Constants.OrderStatusRefundRequested, Constants.OrderStatusRefundDenied, Constants.OrderStatusRefundApprove, Constants.OrderStatusRefunded }.Contains(o.Status)).ToList(),
                    _ => orders // Default to show all orders
                };
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching orders.");
            }

            await LoadCancelReasonsAsync();
        }

        // Method to update the order status
        public async Task<IActionResult> OnPostUpdateStatusAsync(string orderId, string newStatus, string? cancelReasonId)
        {
            // Refresh orders to ensure we're working with the latest data
            await OnGetAsync(CurrentFilter);

            var order = Orders?.FirstOrDefault(o => o.Id == orderId);
            if (order == null ||
                !validStatusTransitions.TryGetValue(order.Status, out var validTransitions) ||
                !validTransitions.Contains(newStatus))
            {
                ModelState.AddModelError(string.Empty, "Invalid status transition.");
                return Page();
            }

            // Create the request body with conditional cancelReasonId
            var requestBody = new
            {
                OrderId = orderId,
                Status = newStatus,
                CancelReasonId = newStatus == Constants.OrderStatusCanceled ? cancelReasonId : null
            };

            var response = await _apiResponseHelper.PatchAsync<object>(
                $"{Constants.ApiBaseUrl}/api/order/status?",
                requestBody
            );

            if (response?.StatusCode == StatusCodeHelper.OK)
            {
                await OnGetAsync(CurrentFilter);  // Refresh orders to show updated status with filter applied
                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "Failed to update order status.");
            }

            return Page();
        }

        public List<string> GetValidStatusTransitions(string currentStatus)
        {
            return validStatusTransitions.ContainsKey(currentStatus) ? validStatusTransitions[currentStatus] : new List<string>();
        }

        private async Task LoadCancelReasonsAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<CancelReason>>(Constants.ApiBaseUrl + "/api/cancelreason?");
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                CancelReasons = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching cancel reasons.");
            }
        }
    }
}
