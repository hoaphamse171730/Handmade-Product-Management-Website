using AutoMapper;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserInfoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserInfoDto> GetUserInfoByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException("invalid_input", "ID is not in a valid GUID format.");
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new BaseException.BadRequestException("bad_request", "User Id cannot be null or empty.");
            }

            var applicationUser = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Include(au => au.UserInfo)
                .FirstOrDefaultAsync(au => au.Id == Guid.Parse(id));

            if (applicationUser == null)
            {
                throw new BaseException.NotFoundException("not_found", "User not found.");
            }
            if (applicationUser.UserInfo == null)
            {
                throw new BaseException.NotFoundException("not_found", "UserInfo not found for this user.");
            }

            var userInfoDto = _mapper.Map<UserInfoDto>(applicationUser.UserInfo);

            return userInfoDto;
        }
    }
}
