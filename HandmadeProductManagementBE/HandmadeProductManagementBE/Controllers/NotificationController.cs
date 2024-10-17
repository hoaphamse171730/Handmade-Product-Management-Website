using Microsoft.AspNetCore.Mvc;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.UserModelViews;
using System.Security.Claims;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
using Microsoft.AspNetCore.Authorization;
using HandmadeProductManagement.Services.Service;


namespace HandmadeProductManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService) => _notificationService = notificationService;


        [Authorize(Roles = "Admin, Customer, Seller")]
        [HttpGet("notification_statuschange")]
        public async Task<IActionResult> GetNewStatusChangeNotification()
        {
            var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = await _notificationService.GetNewStatusChangeNotificationList(id),
                Message = "Success",
            };
            return Ok(response);
        }

        [Authorize(Roles = "Admin, Seller")]
        [HttpGet("notification_review")]
        public async Task<IActionResult> GetNewReviewNotifications()
        {
            var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var notifications = await _notificationService.GetNewReviewNotificationList(id);

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = notifications,
                Message = "Success",
            };

            // Kiểm tra xem notifications có dữ liệu hay không
            if (notifications != null && notifications.Any())
            {
                response.Data = notifications; // Thêm dữ liệu vào phản hồi nếu có
            }
            else
            {
                response.Message = "No new reviews available"; // Thay đổi thông điệp nếu không có dữ liệu
            }

            return Ok(response);
        }
        [Authorize]
        [HttpGet("GetNotificationList")]
        public async Task<IActionResult> GetNotifications()
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            IList<NotificationModel>? notifications = null;
            string message = "Success";

            // Kiểm tra vai trò của người dùng
            if (User.IsInRole("Admin") || User.IsInRole("Seller"))
            {
                notifications = await _notificationService.GetNewReviewNotificationList(userIdFromToken);
                if (notifications == null || !notifications.Any())
                {
                    message = "No new reviews available";
                }
            }

            if (User.IsInRole("Admin") || User.IsInRole("Customer") || User.IsInRole("Seller"))
            {
                var statusChangeNotifications = await _notificationService.GetNewStatusChangeNotificationList(userIdFromToken);
                notifications = notifications?.Concat(statusChangeNotifications).ToList() ?? statusChangeNotifications;
            }

            var orderNotifications = await _notificationService.GetNewOrderNotificationList(userIdFromToken);
            notifications = notifications?.Concat(orderNotifications).ToList() ?? orderNotifications;

            var replyNotifications = await _notificationService.GetNewReplyNotificationList(userIdFromToken);
            notifications = notifications?.Concat(replyNotifications).ToList() ?? replyNotifications;

            var response = new BaseResponse<IList<NotificationModel>>
            {
                Code = "200",
                StatusCode = StatusCodeHelper.OK,
                Data = notifications,
                Message = message,
            };

            return Ok(response);
        }
    }
}
