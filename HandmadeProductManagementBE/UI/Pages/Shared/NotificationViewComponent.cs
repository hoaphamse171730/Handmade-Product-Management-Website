using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Store;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
using Microsoft.AspNetCore.Mvc;
namespace UI.Pages.Shared
{
    public class NotificationListViewComponent : ViewComponent
    {
        private readonly ApiResponseHelper _apiResponseHelper;

        public NotificationListViewComponent(ApiResponseHelper apiResponseHelper)
        {
            _apiResponseHelper = apiResponseHelper;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var (notifications, count) = await GetNotificationListAsync();
            ViewData["NotificationCount"] = count;
            return View(notifications);
        }
        private async Task<(List<NotificationModel>, int)> GetNotificationListAsync()
        {
            var response = await _apiResponseHelper.GetAsync<List<NotificationModel>>($"{Constants.ApiBaseUrl}/api/notification/getnotificationlist");
            if (response.StatusCode == StatusCodeHelper.OK && response.Data != null)
            {
                return (response.Data, response.Data.Count);
            }
            return (new List<NotificationModel>(), 0);
        }
    }
}
