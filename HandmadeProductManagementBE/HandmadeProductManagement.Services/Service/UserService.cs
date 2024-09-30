using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;

using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<UserResponseModel>> GetAll()
        {
            IQueryable<ApplicationUser> query = _unitOfWork.GetRepository<ApplicationUser>().Entities;

            // Map ApplicationUser to UserResponseModel

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
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
                }).ToListAsync();

            return user as IList<UserResponseModel>;  // Cast List to IList
        }

        public async Task<UserResponseByIdModel> GetById(string Id)
        {
            // Ensure the id is a valid Guid
            if (!Guid.TryParse(Id, out Guid userId))
            {
                return null; 
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
                .FirstOrDefaultAsync(); // Get the first or default user

            return user;
        }

        public async Task<UpdateUserResponseModel?> UpdateUser(string id, UpdateUserDTO updateUserDTO)
        {

            // Ensure the id is a valid Guid
            if (!Guid.TryParse(id, out Guid userId))
            {
                return null; 
            }

            // Get the user by ID using LINQ
            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync(); 

            if (user == null)
            {
                return null; 
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
                return false;
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
               .Entities
               .Where(u => u.Id == userId)
               .FirstOrDefaultAsync();


            if (user == null)
            {
                return false;
            }

            user.Status = "inactive";
            user.DeletedBy = "admin";
            user.DeletedTime = DateTime.UtcNow;

            // Update the user in the repository
            _unitOfWork.GetRepository<ApplicationUser>().Update(user);
            await _unitOfWork.SaveAsync();


            return true;
        }

        public async Task<bool> ReverseDeleteUser(string Id)
        {
            if (!Guid.TryParse(Id, out Guid userId))
            {
                return false;
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
               .Entities
               .Where(u => u.Id == userId)
               .FirstOrDefaultAsync();

            if (user == null || user.Status == "active")
            {
                return false;
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
