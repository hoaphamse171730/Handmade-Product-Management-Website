﻿using Microsoft.AspNetCore.Mvc;
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
using HandmadeProductManagement.Contract.Repositories.Entity;

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
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public bool HasNextPage { get; set; } = true;

        public async Task OnGetAsync(string? filter, int pageNumber = 1, int pageSize = 12)
        {
            CurrentFilter = filter ?? "All";

            PageNumber = pageNumber;
            PageSize = pageSize;

            var response = await _apiResponseHelper.GetAsync<List<OrderByUserDto>>(Constants.ApiBaseUrl + $"/api/order/user?pageNumber={PageNumber}&pageSize={PageSize}");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                var orders = response.Data.OrderByDescending(o => o.OrderDate).ToList();
                Orders = CurrentFilter switch
                {
                    "Pending" => orders.Where(o => o.Status == "Pending").ToList(),
                    "Awaiting Payment" => orders.Where(o => o.Status == "Awaiting Payment").ToList(),
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
                Status = Constants.OrderStatusCanceled,
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

        public async Task<IActionResult> OnPutUpdateOrderAsync(string orderId,[FromBody] UpdateOrderDto updateOrderDto)
        {
            // Make the PUT request to update the order
            var response = await _apiResponseHelper.PutAsync<bool>(Constants.ApiBaseUrl + $"/api/order/{orderId}", updateOrderDto);

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data)
            {
                // Refresh the orders list after successful update
                await OnGetAsync(CurrentFilter);
                return new JsonResult(new { success = true });
            }
            else
            {
                return new JsonResult(new { success = false, message = response?.Message ?? "An error occurred while updating the order." });
            }
        }

    }
}
