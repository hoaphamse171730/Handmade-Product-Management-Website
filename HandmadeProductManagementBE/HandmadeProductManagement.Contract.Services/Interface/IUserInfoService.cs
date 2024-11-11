

using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using Microsoft.AspNetCore.Http;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IUserInfoService
    {
        Task<UserInfoDto> GetUserInfoByIdAsync(string id);
        Task<bool> PatchUserInfoAsync(string id, UserInfoForUpdateDto request);
        Task<bool> UploadUserAvatar(IFormFile file, string productId);
    }
}
