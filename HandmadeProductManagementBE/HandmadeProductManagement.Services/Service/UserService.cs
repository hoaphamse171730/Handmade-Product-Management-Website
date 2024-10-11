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
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Http;
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
                    CartId = user.CartId,
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
                    CartId = user.CartId,

                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "user not found");
            }
            return user;

        }
        public async Task<UpdateUserResponseModel?> UpdateUser(string id, UpdateUserDTO updateUserDTO)
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
            var existingUsername = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .AnyAsync(u => u.UserName == updateUserDTO.UserName && u.Id != userId);
            if(existingUsername)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Username already exists");
            }

            var existingUserWithSameEmail = await _unitOfWork.GetRepository<ApplicationUser>()
      .Entities
      .AnyAsync(u => u.Email == updateUserDTO.Email && u.Id != userId);
            if (existingUserWithSameEmail)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Email already exists");
            }

            var existingUserWithSamePhoneNumber = await _unitOfWork.GetRepository<ApplicationUser>()
        .Entities
        .AnyAsync(u => u.PhoneNumber == updateUserDTO.PhoneNumber && u.Id != userId);
            if (existingUserWithSamePhoneNumber)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Phone number already exists");
            }

            user.UserName = updateUserDTO.UserName;
            user.Email = updateUserDTO.Email;
            user.PhoneNumber = updateUserDTO.PhoneNumber;
            user.TwoFactorEnabled = updateUserDTO.TwoFactorEnabled;
            user.NormalizedUserName = updateUserDTO.UserName.ToUpper();
            user.NormalizedEmail = updateUserDTO.Email.ToUpper();
            user.LastUpdatedTime = DateTime.UtcNow;

            _unitOfWork.GetRepository<ApplicationUser>().Update(user);
            await _unitOfWork.SaveAsync();

            var updatedUserResponse = new UpdateUserResponseModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            return updatedUserResponse;
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


            if (user == null || user.Status == "inactive")
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found");
            }

            user.Status = "inactive";
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

            if(shopIds.IsNullOrEmpty())
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

            // Lấy danh sách đơn hàng của người dùng, sắp xếp theo LastUpdatedTime (tăng dần)
            var orders = await _unitOfWork.GetRepository<Order>()
                .Entities
                .Where(o => o.UserId == userId)
                .OrderBy(o => o.LastUpdatedTime) // Sắp xếp theo LastUpdatedTime tăng dần
                .Include(o => o.User)
                .ToListAsync();

            // Tạo danh sách thông báo cho các đơn hàng
            var notifications = orders.Select(order => new NotificationModel
            {
                Id = order.Id,
                Message = $"Bạn có đơn hàng mới từ {order.CustomerName} với trạng thái: {order.Status} vào ngày: {order.LastUpdatedTime.ToString("dd/MM/yyyy")}",
                Tag = "Order",
                URL = urlroot + $"/api/order/{order.Id}"
            }).ToList();

            return notifications;
        }


        public async Task<IList<NotificationModel>> GetNewReviewNotificationList(string Id)
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
                URL = Url + $"/api/review/{review.Id}"
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

            var urlroot = "https://localhost:7159";

            // Lấy tất cả các reply mới cho những review của khách hàng, sắp xếp theo thời gian tạo reply (giảm dần)
            var replies = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .Where(rep => reviews.Select(r => r.Id).Contains(rep.ReviewId) && rep.Date >= DateTime.UtcNow.AddDays(-2)) // Lọc theo thời gian tạo reply trong 2 ngày gần nhất
                .Include(rep => rep.Review) // Bao gồm review
                .ThenInclude(r => r.Product) // Bao gồm sản phẩm
                .OrderByDescending(rep => rep.Date) // Sắp xếp theo thời gian tạo reply (giảm dần)
                .ToListAsync();

            if (replies == null || !replies.Any())
            {
                return new List<NotificationModel>();
            }

            // Tạo thông báo cho từng phản hồi mới
            var replyNotifications = replies.Select(reply => new NotificationModel
            {
                Id = reply.Id,
                Message = $"Bạn đã nhận được phản hồi mới cho review sản phẩm {reply.Review.Product.Name}",
                Tag = "Reply",
                URL = urlroot + $"/api/reply/{reply.Id}"
            }).ToList();

            return replyNotifications;
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

            // Lấy status của orders, sắp xếp theo ChangeTime (giảm dần)
            var status = await _unitOfWork.GetRepository<StatusChange>()
                .Entities
                .Where(s => orders.Contains(s.OrderId))
                .Include(s => s.Order)
                .OrderByDescending(s => s.ChangeTime) // Sắp xếp theo thời gian thay đổi trạng thái (giảm dần)
                .ToListAsync();

            // Tạo thông báo phản hồi
            var notifications = status.Select(status => new NotificationModel
            {
                Id = status.Id,
                Message = $"Đơn hàng của bạn được {status.Status} lúc {status.ChangeTime.ToString("dd/MM/yyyy HH:mm")}",
                Tag = "StatusChange",
                URL = Url + $"/api/statuschange/order/{status.OrderId}"
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

            if (user == null || user.Status == "active")
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "User not found or already active");
            }

            user.Status = "active";
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
