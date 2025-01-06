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
                // Tạo một danh sách mới để chứa dữ liệu đã đảo ngược
                List<NotificationModel> reversedData = new List<NotificationModel>();

                // Duyệt qua danh sách từ cuối đến đầu và thêm vào danh sách mới
                for (int i = response.Data.Count - 1; i >= 0; i--)
                {
                    reversedData.Add(response.Data[i]);
                }

                // Lọc ra chỉ một thông báo với tag = "StatusChange"
                var statusChangeNotification = reversedData.FirstOrDefault(n => n.Tag == "StatusChange");

                // Nếu tìm thấy thông báo "StatusChange" đầu tiên
                if (statusChangeNotification != null)
                {
                    // Lọc các thông báo "StatusChange" còn lại
                    reversedData = reversedData.Where(n => !(n.Tag == "StatusChange" && n != statusChangeNotification)).ToList();
                }

                // Đếm các thông báo chưa đọc
                var unRead = reversedData.Count(n => !n.HaveSeen);

                return (reversedData, reversedData.Count);
            }
            return (new List<NotificationModel>(), 0);
        }

    }
}
