

using HandmadeProductManagement.ModelViews.UserInfoModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IUserInfoService
    {
        Task<UserInfoDto> GetUserInfoByIdAsync(string id);
    }
}
