using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.Contract.Repositories.Entity;
using static System.Formats.Asn1.AsnWriter;
using HandmadeProductManagement.Core.Utils;
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
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),"Please check UserID");
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

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync(); 

            if (user == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), "user not found");
            }

            var updateValidation = _updateValidator.Validate(updateUserDTO);
            if (!updateValidation.IsValid)
            {
                throw new ValidationException(updateValidation.Errors);
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

        public async Task<List<NotificationModel>> GetNotificationList(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Invalid userID");
            }
           
            var shop = await _unitOfWork.GetRepository<Shop>()
                .Entities
                .Where(shop => shop.UserId == userId)
                .Select(shop => shop.Id)
                .ToListAsync();
          
            var reviews = await _unitOfWork.GetRepository<Review>()
                .Entities
                .Include(r => r.User) 
                .Where(review => shop.Contains(review.ProductId) && review.Reply == null)
                .ToListAsync();

            var notifications = reviews.Select(review => new NotificationModel
            {
                Id = review.Id,
                Message = $"Sản phẩm của bạn đã được {review.User.UserName} review",
                Tag = "Review",
                URL = $"api/review/{review.Id}"
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


    }
}
