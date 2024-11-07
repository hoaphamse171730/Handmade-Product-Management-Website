using HandmadeProductManagement.ModelViews.UserInfoImageModelViews;
using Microsoft.AspNetCore.Http;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IUserInfoImageService
    {
        Task<bool> UploadUserInfoImage(IFormFile file, string UserinfoId);

        Task<bool> DeleteUserInfoImage(string ImageId);

        Task<IList<userinfoimage>> GetUserInfoImageById(string id);
    }
}
