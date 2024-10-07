using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.NotificationModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IUserService
    {
        Task<IList<UserResponseModel>> GetAll();

        Task<UserResponseByIdModel> GetById(string Id);

        Task<UpdateUserResponseModel> UpdateUser(string id, UpdateUserDTO updateUserDTO);

        Task<bool> DeleteUser(string Id);

        Task<bool> ReverseDeleteUser(string Id);

        Task<IList<NotificationModel>> GetNotificationList(string Id);
    }
}

