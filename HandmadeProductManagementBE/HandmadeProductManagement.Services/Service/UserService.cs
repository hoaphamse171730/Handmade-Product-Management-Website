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
namespace HandmadeProductManagement.Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator<UpdateUserDTO> _updateValidator;
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

            var reviews = await _unitOfWork.GetRepository<Review>()
                .Entities
                .Where(r => shopIds.Contains(r.Product.ShopId))
                .Include(r => r.User)
                .ToListAsync();

            var replies = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .Where(rep => reviews.Select(r => r.Id).Contains(rep.ReviewId)) 
                .ToListAsync();

            var nonReplies = reviews
                .Where(r => !replies.Any(rep => rep.ReviewId == r.Id))
                .ToList();

            var notifications = nonReplies.Select(nonReplies => new NotificationModel
            {
                Id = nonReplies.Id,
                Message = $"Sản phẩm của bạn đã được {nonReplies.User.UserName} review",
                Tag = "Review",
                URL = $"api/review/{nonReplies.Id}"
            }).ToList();

            return notifications;
        }

        public async Task<IList<NotificationModel>> GetNewOrderNotificationList(string Id)
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

            // Lấy danh sách đơn hàng mới trong vòng 2 ngày
            var currentDate = DateTime.UtcNow;
            var twoDaysAgo = currentDate.AddDays(-2);

            var newOrders = await _unitOfWork.GetRepository<Order>()
            .Entities
            .Where(o => o.OrderDetails.Any(od => shopIds.Contains(od.ProductItem.Product.ShopId)) && o.OrderDate >= twoDaysAgo && o.OrderDate <= currentDate)
            .Include(o => o.User)
            .Include(o => o.OrderDetails) // Bao gồm OrderDetails để truy cập ProductItem
            .ThenInclude(od => od.ProductItem) // Bao gồm ProductItem trong OrderDetail
            .ThenInclude(pi => pi.Product) // Bao gồm Product trong ProductItem
            .ToListAsync();


            var orderNotifications = newOrders.Select(order => new NotificationModel
            {
                Id = order.Id,
                Message = $"Bạn có một đơn hàng mới từ {order.User.UserName}",
                Tag = "NewOrder",
                URL = $"api/order/{order.Id}"
            }).ToList();

            return orderNotifications;
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

            // Lấy tất cả các reply mới cho những review của khách hàng
            var replies = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .Where(rep => reviews.Select(r => r.Id).Contains(rep.ReviewId) && rep.Date >= DateTime.UtcNow.AddDays(-2)) // Lọc theo thời gian tạo reply trong 2 ngày gần nhất
                .Include(rep => rep.Review) // Bao gồm review
                .ThenInclude(r => r.Product) // Bao gồm sản phẩm
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
                URL = $"api/reply/{reply.Id}"
            }).ToList();

            return replyNotifications;
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
    }
}
