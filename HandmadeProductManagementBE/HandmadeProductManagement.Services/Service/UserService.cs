using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using HandmadeProductManagement.Core.Utils;
namespace HandmadeProductManagement.Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UpdateUserDTO> _updateValidator;
        private string Url = "https://" + "localhost:44328/";
        public UserService(IUnitOfWork unitOfWork, IValidator<UpdateUserDTO> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _updateValidator = updateValidator;
        }

        public async Task<IList<UserResponseModel>> GetAll()
        {

            var users = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Select(user => new UserResponseModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    CreatedBy = user.CreatedBy,
                    LastUpdatedBy = user.LastUpdatedBy,
                    DeletedBy = user.DeletedBy,
                    CreatedTime = user.CreatedTime,
                    LastUpdatedTime = user.LastUpdatedTime,
                    DeletedTime = user.DeletedTime,
                    Status = user.Status,
                })
                .ToListAsync();

            if (users == null || !users.Any())
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Please check UserID");
            }

            return users;

        }

        public async Task<UserResponseByIdModel> GetById(string Id)
        {
            // Ensure the id is a valid Guid
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.Id == userId)
                .Select(user => new UserResponseByIdModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    NormalizedUserName = user.NormalizedUserName,
                    Email = user.Email,
                    NormalizedEmail = user.NormalizedEmail,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnd = user.LockoutEnd,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,

                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "user not found");
            }
            return user;

        }
        public async Task<bool> UpdateUser(string id, UpdateUserDTO updateUserDTO)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }
            //query
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
            //check user found
            if (user == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "user not found");
            }
            // check format DTO by fluent validation
            var updateValidation = _updateValidator.Validate(updateUserDTO);
            if (!updateValidation.IsValid)
            {
                throw new ValidationException(updateValidation.Errors);
            }
            // check existing unique fields
            if (!string.IsNullOrEmpty(updateUserDTO.UserName))
            {
                var existingUsername = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                    .AnyAsync(u => u.UserName == updateUserDTO.UserName && u.Id != userId);
                if (existingUsername)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Username already exists");
                }
                user.UserName = updateUserDTO.UserName;
                user.NormalizedUserName = updateUserDTO.UserName.ToUpper();
            }

            if (!string.IsNullOrEmpty(updateUserDTO.Email))
            {
                var existingUserWithSameEmail = await _unitOfWork.GetRepository<ApplicationUser>()
                    .Entities
                    .AnyAsync(u => u.Email == updateUserDTO.Email && u.Id != userId);
                if (existingUserWithSameEmail)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Email already exists");
                }
                user.Email = updateUserDTO.Email;
                user.NormalizedEmail = updateUserDTO.Email.ToUpper();
            }

            if (!string.IsNullOrEmpty(updateUserDTO.PhoneNumber))
            {
                var existingUserWithSamePhoneNumber = await _unitOfWork.GetRepository<ApplicationUser>()
                    .Entities
                    .AnyAsync(u => u.PhoneNumber == updateUserDTO.PhoneNumber && u.Id != userId);
                if (existingUserWithSamePhoneNumber)
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Phone number already exists");
                }
                user.PhoneNumber = updateUserDTO.PhoneNumber;
            } 
            user.TwoFactorEnabled = updateUserDTO.TwoFactorEnabled;
            user.LastUpdatedTime = DateTime.UtcNow;
            await _unitOfWork.GetRepository<ApplicationUser>().UpdateAsync(user);
            await _unitOfWork.SaveAsync();
            return true;
        }


        public async Task<bool> DeleteUser(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
               .Entities
               .Where(u => u.Id == userId)
               .FirstOrDefaultAsync();


            if (user == null || user.Status == "Inactive")
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found");
            }

            user.Status = "Inactive";
            user.DeletedBy = "admin";
            user.DeletedTime = DateTime.UtcNow;


            _unitOfWork.GetRepository<ApplicationUser>().Update(user);
            await _unitOfWork.SaveAsync();


            return true;
        }


        public async Task<IList<NotificationModel>> GetNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            var shopIds = await _unitOfWork.GetRepository<Shop>()
                .Entities
                .Where(shop => shop.UserId == userId)
                .Select(shop => shop.Id)
                .ToListAsync();

            if (shopIds.IsNullOrEmpty())
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found");
            }

            var review = await _unitOfWork.GetRepository<Review>()
                .Entities
                .Where(r => r.UserId == userId && r.Reply == null)
                .Include(r => r.User)
                .ToListAsync();

            var notifications = review.Select(review => new NotificationModel
            {
                Id = review.Id,
                Message = $"Sản phẩm của bạn đã được {review.User.UserName} review",
                Tag = "Review",
                URL = $"/api/review/{review.Id}"
            }).ToList();

            return notifications;
        }

        public async Task<IList<NotificationModel>> GetNewOrderNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            var urlroot = "https://localhost:7159";
            var fromDate = DateTime.UtcNow.AddDays(-2); // Lọc đơn hàng trong vòng 2 ngày

            // Lấy danh sách đơn hàng trong vòng 2 ngày dựa trên ShopId của người dùng (người bán), sắp xếp theo LastUpdatedTime (tăng dần)
            var orders = await _unitOfWork.GetRepository<OrderDetail>()
                .Entities
                .Where(od => od.ProductItem.Product.Shop.UserId == userId && od.Order.OrderDate >= fromDate)
                .OrderBy(od => od.Order.LastUpdatedTime) // Sắp xếp theo LastUpdatedTime tăng dần
                .Include(od => od.Order)
                .Include(od => od.Order.User) // Bao gồm thông tin người mua
                .ToListAsync();

            if (!orders.Any())
            {
                return new List<NotificationModel>(); // Không có đơn hàng trong khoảng thời gian
            }

            // Tạo danh sách thông báo cho các đơn hàng
            var notifications = orders.Select(orderDetail => new NotificationModel
            {
                Id = orderDetail.Order.Id,
                Message = $"Bạn có đơn hàng mới từ {orderDetail.Order.CustomerName} với trạng thái: {orderDetail.Order.Status} vào ngày: {orderDetail.Order.LastUpdatedTime.ToString("dd/MM/yyyy")}",
                Tag = "Order",
                URL = urlroot + $"/api/order/{orderDetail.Order.Id}"
            }).ToList();

            return notifications;
        }

        public async Task<IList<NotificationModel>> GetNewReplyNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            // Lấy danh sách các review của người dùng
            var reviews = await _unitOfWork.GetRepository<Review>()
                .Entities
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (reviews == null || !reviews.Any())
            {
                return new List<NotificationModel>();
            }
            var twoDaysAgo = DateTime.Now.AddDays(-2);

            
            // Lấy tất cả các reply mới cho những review của khách hàng
            var replies = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .Where(rep => reviews.Select(r => r.Id).Contains(rep.ReviewId) && rep.Date.Value.Date >= twoDaysAgo.Date) // Lọc theo thời gian tạo reply trong 2 ngày gần nhất
                .Include(rep => rep.Review) // Bao gồm review
                    .ThenInclude(r => r.Product) // Bao gồm sản phẩm
                    .ThenInclude(p => p.Shop)  // Nạp thông tin shop từ sản phẩm
                    .ThenInclude(s => s.User)  // Nạp thông tin user (chủ shop) từ shop
                    .ThenInclude(u => u.UserInfo) // Nạp thông tin UserInfo từ User
                .ToListAsync();

            if (replies == null || !replies.Any())
            {
                return new List<NotificationModel>();
            }

            // Tạo thông báo cho từng phản hồi mới
            var replyNotifications = replies.Select(reply => new NotificationModel
            {
                Id = reply.Id,
                Message = $"Bạn đã nhận được phản hồi mới cho review sản phẩm {reply.Review.Product.Shop.User.UserInfo.FullName}",
                Tag = "Reply",
                URL = Url + $"api/reply/{reply.Id}"
            }).ToList();

            return replyNotifications;
        }

        public async Task<IList<NotificationModel>> GetNewReviewNotificationList(string Id)
        {

            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            var shopID = await _unitOfWork.GetRepository<Shop>()
                .Entities
                .Where(r => r.UserId == userId)
                .Select(r => r.Id)
                .ToListAsync();
            if (shopID.IsNullOrEmpty())
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found");
            }

            var twoDaysAgo = DateTime.Now.AddDays(-2);

            var review = await _unitOfWork.GetRepository<Review>()
                 .Entities
                 .Where(r => r.Reply == null && r.Date.Value.Date >= twoDaysAgo.Date)  // Lọc các review không có phản hồi và trong hai ngày qua
                 .Include(r => r.User)  // Nạp thông tin người dùng từ review
                    .ThenInclude(u => u.UserInfo)  // Nạp thông tin UserInfo từ User của người viết review
                 //.Include(r => r.Product)  // Nạp thông tin sản phẩm từ review
                 //   .ThenInclude(p => p.Shop)  // Nạp thông tin shop từ sản phẩm
                 .ToListAsync();

            var notifications = review.Select(r => new NotificationModel
            {
                Id = r.Id,
                Message = $"Sản phẩm của bạn đã được {r.User.UserInfo.FullName} review",  // Lấy FullName từ người dùng trong Review
                Tag = "Review",
                URL = Url + $"api/review/{r.Id}"
            }).ToList();

            return notifications;
        }

        public async Task<IList<NotificationModel>> GetNewStatusChangeNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            // Lấy danh sách order của người dùng
            var orders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(o => o.UserId == userId)
                .Select(o => o.Id)
                .ToListAsync();

            if (orders == null || !orders.Any())
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found");
            }

            var twoDaysAgo = DateTime.Now.AddDays(-2);

            // Lấy status của orders
            var status = await _unitOfWork.GetRepository<StatusChange>()
                .Entities
                .Where(s => orders.Contains(s.OrderId) && s.ChangeTime.Date >= twoDaysAgo)
                .Include(s => s.Order)
                .OrderBy(s => s.ChangeTime) // Sắp xếp theo thời gian thay đổi trạng thái (tăng dần)
                .ToListAsync();

            // Định nghĩa múi giờ UTC+7 (Vietnam)
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Tạo thông báo phản hồi
            var notifications = status.Select(status => {
                // Chuyển đổi thời gian từ UTC sang UTC+7
                var changeTimeInVietnam = TimeZoneInfo.ConvertTimeFromUtc(status.ChangeTime, vietnamTimeZone);

                return new NotificationModel
                {
                    Id = status.Id,
                    Message = $"Đơn hàng của bạn được {status.Status} lúc {changeTimeInVietnam.ToString("dd/MM/yyyy HH:mm")}",
                    Tag = "StatusChange",
                    URL = Url + $"/api/statuschange/order/{status.OrderId}"
                };
            }).ToList();

            return notifications;
        }





        public async Task<bool> ReverseDeleteUser(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
               .Entities
               .Where(u => u.Id == userId)
               .FirstOrDefaultAsync();

            if (user == null || user.Status == "Active")
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found or already active");
            }

            user.Status = "Active";
            user.DeletedBy = null;
            user.DeletedTime = null;

            _unitOfWork.GetRepository<ApplicationUser>().Update(user);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<UpdateUserResponseModel?> UpdateUserProfile(string id, UpdateUserDTO updateUserProfileDTO)
        {
            if (!Guid.TryParse(id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found");
            }

            if (!string.IsNullOrEmpty(updateUserProfileDTO.UserName))
            {
                user.UserName = updateUserProfileDTO.UserName;
                user.NormalizedUserName = updateUserProfileDTO.UserName.ToUpper();
            }
            _unitOfWork.GetRepository<ApplicationUser>().Update(user);
            await _unitOfWork.SaveAsync();

            var updatedUserResponse = new UpdateUserResponseModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
            };

            return updatedUserResponse;
        }

        private async Task<string> SaveAvatarFile(IFormFile avatarFile)
        {
            // Implement logic to save the avatar file locally or in cloud storage (e.g., AWS S3, Azure Blob Storage)
            // Return the URL to the saved file
            var filePath = Path.Combine("wwwroot/images/avatars", avatarFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatarFile.CopyToAsync(stream);
            }

            return $"/images/avatars/{avatarFile.FileName}"; // Return the relative path or URL of the image
        }

    }
}