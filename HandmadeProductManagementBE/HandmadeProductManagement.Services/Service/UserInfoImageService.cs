using Firebase.Auth;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserInfoImageModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class UserInfoImageService : IUserInfoImageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserInfoImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> UploadUserInfoImage(IFormFile file, string UserInfoId)
        {
            if (file == null || file.Length == 0)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageFileNotFound);
            }

            var userinfo = await _unitOfWork.GetRepository<UserInfo>()
                .Entities
                .FirstOrDefaultAsync(u => u.Id == UserInfoId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

            var uploadImageService = new ManageFirebaseImageService();

            using (var stream = file.OpenReadStream())
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var imageUrl = await uploadImageService.UploadFileAsync(stream, fileName);

                // Cập nhật AvatarUrl của userinfo
                userinfo.AvatarUrl = imageUrl;

                var userInfoImage = new UserInfoImage
                {
                    Url = imageUrl,
                    UserInfoId = UserInfoId
                };

                await _unitOfWork.GetRepository<UserInfoImage>().InsertAsync(userInfoImage);

                // Cập nhật thay đổi userinfo
                _unitOfWork.GetRepository<UserInfo>().Update(userinfo);
                await _unitOfWork.SaveAsync();
            }

            return true;
        }

        public async Task<bool> DeleteUserInfoImage(string ImageId)
        {
            var userInfoImage = await _unitOfWork.GetRepository<UserInfoImage>()
            .Entities
                .Where(ui => ui.Id == ImageId)
                .FirstOrDefaultAsync()
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageImageNotFound);

            var deleteImageService = new ManageFirebaseImageService();
            await deleteImageService.DeleteFileAsync(userInfoImage.Url);

            _unitOfWork.GetRepository<UserInfoImage>().Delete(userInfoImage.Id);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IList<userinfoimage>> GetUserInfoImageById(string id)
        {
            var images = await _unitOfWork.GetRepository<UserInfoImage>()
                .Entities.Where(ui => ui.UserInfoId == id)
                .Select(ui => new userinfoimage
                {
                    Id = ui.Id,
                    Url = ui.Url,
                }).ToListAsync();
            return images;
        }

    }
}
