using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            var result = await query.Select(user => new UserResponseModel
            {
                Id = user.Id.ToString()   // Convert Guid to string to match UserResponseModel
            }).ToListAsync();

            return result as IList<UserResponseModel>;  // Cast List to IList
        }

    }
}
