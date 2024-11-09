using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Base;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Core.Common;
using Google.Apis.Storage.v1.Data;

namespace HandmadeProductManagement.Services.Service
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<NotificationModel>> GetNewReviewNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var shopID = await _unitOfWork.GetRepository<Shop>()
                .Entities
                .Where(r => r.UserId == userId)
                .Select(r => r.Id)
                .ToListAsync();

            //if (shopID == null || !shopID.Any())
            //{
            //    throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);
            //}
            if (shopID == null || !shopID.Any())
            {
                return new List<NotificationModel>();
            }

            var twoDaysAgo = DateTime.UtcNow.AddDays(-2); // Use UTC for consistency

            var review = await _unitOfWork.GetRepository<Review>()
                 .Entities
                 .Where(r => r.Reply == null && r.Date.Date >= twoDaysAgo.Date)  // Lọc các review không có phản hồi và trong hai ngày qua
                 .Include(r => r.User)  // Nạp thông tin người dùng từ review
                    .ThenInclude(u => u!.UserInfo)  // Nạp thông tin UserInfo từ User của người viết review
                                                    //.Include(r => r.Product)  // Nạp thông tin sản phẩm từ review
                                                    //   .ThenInclude(p => p.Shop)  // Nạp thông tin shop từ sản phẩm
                 .ToListAsync();

            var notifications = review.Select(r => new NotificationModel
            {
                Id = r.Id,
                Message = $"Your product has been reviewed by {r.User?.UserInfo.FullName ?? "Unknown User"}",  // Lấy FullName từ người dùng trong Review
                Tag = Constants.NotificationTagReview,
                URL = Constants.ApiBaseUrl + $"/Review/Details/{r.Id}"
            }).ToList();

            return notifications;
        }

        public async Task<IList<NotificationModel>> GetNewOrderNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var fromDate = DateTime.UtcNow.AddDays(-2); // Filter orders from the last 2 days

            // Lấy danh sách đơn hàng trong vòng 2 ngày dựa trên ShopId của người dùng (người bán), sắp xếp theo LastUpdatedTime (tăng dần)
            var orders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(o => o.OrderDetails.Any(od =>
                                            od.ProductItem != null
                                         && od.ProductItem.Product != null
                                         && od.ProductItem.Product.Shop != null
                                         && od.ProductItem.Product.Shop.UserId == userId)
                                && o.OrderDate >= fromDate)
                .OrderBy(o => o.LastUpdatedTime) // Sắp xếp theo LastUpdatedTime tăng dần
                .Include(o => o.User) // Bao gồm thông tin người mua
                .ToListAsync();

            if (!orders.Any())
            {
                return []; // Không có đơn hàng trong khoảng thời gian
            }

            // Tạo danh sách thông báo cho các đơn hàng
            var notifications = orders.Select(order => new NotificationModel
            {
                Id = order.Id,
                Message = $"Bạn có đơn hàng mới từ {order.CustomerName} với trạng thái: {order.Status} vào ngày: {order.LastUpdatedTime.ToString("dd/MM/yyyy")}",
                Tag = Constants.NotificationTagOrder,
                URL = Constants.ApiBaseUrl + $"/api/order/{order.Id}"
            }).ToList();

            return notifications;
        }

        public async Task<IList<NotificationModel>> GetNewStatusChangeNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat); // Use constant for invalid GUID format
            }

            // Retrieve the list of orders for the user
            var orders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(o => o.UserId == userId)
                .Select(o => o.Id)
                .ToListAsync();

            //if (orders == null || !orders.Any())
            //{
            //    throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound); // Use constant for user not found
            //}
            if (orders == null || !orders.Any())
            {
                return new List<NotificationModel>();
            }

            var twoDaysAgo = DateTime.UtcNow.AddDays(-2); // Use UTC for consistency

            // Get the status changes for the orders
            var statusChanges = await _unitOfWork.GetRepository<StatusChange>()
                .Entities
                .Where(s => orders.Contains(s.OrderId) && s.ChangeTime.Date >= twoDaysAgo)
                .Include(s => s.Order)
                .OrderBy(s => s.ChangeTime) // Order by change time ascending
                .ToListAsync();

            // Define UTC+7 timezone (Vietnam)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById(Constants.TimeZoneSEAsiaStandard);

            // Create response notifications
            var notifications = statusChanges.Select(status => {
                // Convert time from UTC to UTC+7
                var changeTimeInVietnam = TimeZoneInfo.ConvertTimeFromUtc(status.ChangeTime, vietnamTimeZone);

                return new NotificationModel
                {
                    Id = status.Id,
                    Message = $"Your order is {status.Status} at {changeTimeInVietnam.ToString(Constants.DateTimeFormat)}",
                    Tag = Constants.NotificationTagStatusChange, // Use constant for notification tag
                    URL = Constants.FrontUrl + $"/Order/OrderDetail?orderId={status.OrderId}"
                };
            }).ToList();

            return notifications;
        }

        public async Task<IList<NotificationModel>> GetNewReplyNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Get the list of reviews for the user
            var reviews = await _unitOfWork.GetRepository<Review>()
                .Entities
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (reviews == null || !reviews.Any())
            {
                return new List<NotificationModel>();
            }

            var twoDaysAgo = DateTime.UtcNow.AddDays(-2); // Use UTC for consistency

            // Get all new replies for the user's reviews in the last 2 days
            var replies = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .Where(rep => reviews.Select(r => r.Id).Contains(rep.ReviewId) && (rep.Date.HasValue && rep.Date.Value.Date >= twoDaysAgo.Date)) // Lọc theo thời gian tạo reply trong 2 ngày gần nhất
                .Include(rep => rep.Review) // Bao gồm review
                    .ThenInclude(r => r!.Product) // Bao gồm sản phẩm
                    .ThenInclude(p => p!.Shop)  // Nạp thông tin shop từ sản phẩm
                    .ThenInclude(s => s!.User)  // Nạp thông tin user (chủ shop) từ shop
                    .ThenInclude(u => u!.UserInfo) // Nạp thông tin UserInfo từ User
                .ToListAsync();

            if (replies == null || replies.Count == 0)
            {
                return [];
            }

            // Create notifications for each new reply
            var replyNotifications = replies.Select(reply => new NotificationModel
            {
                Id = reply.Id,
                Message = $"Bạn đã nhận được phản hồi mới cho review sản phẩm {reply?.Review?.Product?.Shop?.User?.UserInfo.FullName ?? "Unknown User"}",
                Tag = Constants.NotificationTagReply,
                URL = Constants.ApiBaseUrl + $"api/reply/{reply!.Id}"
            }).ToList();

            return replyNotifications;
        }

        
    }
}
