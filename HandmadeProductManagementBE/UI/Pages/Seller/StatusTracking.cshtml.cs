using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.StatusChangeModelViews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UI.Pages.Seller
{
    public class StatusTrackingModel : PageModel
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public StatusTrackingModel(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper ?? throw new ArgumentNullException(nameof(apiResponseHelper));
        }

        public List<StatusChangeDto>? StatusChanges { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public async Task OnGetAsync(string orderId)
        {
            OrderId = orderId;
            await FetchStatusChangesAsync(orderId);
        }

        private async Task FetchStatusChangesAsync(string orderId)
        {
            var response = await _apiResponseHelper.GetAsync<List<StatusChangeDto>>(Constants.ApiBaseUrl + $"/api/StatusChange/Order/{orderId}");

            if (response?.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                StatusChanges = response.Data;
            }
            else
            {
                ModelState.AddModelError(string.Empty, response?.Message ?? "An error occurred while fetching status changes.");
            }
        }
    }
}
