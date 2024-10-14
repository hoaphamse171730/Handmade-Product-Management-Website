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

        Task<bool> UpdateUser(string id, UpdateUserDTO updateUserDTO);

        Task<bool> DeleteUser(string Id);

        Task<bool> ReverseDeleteUser(string Id);

        Task<IList<NotificationModel>> GetNotificationList(string Id);
        Task<IList<NotificationModel>> GetNewReviewNotificationList(string Id);

        Task<IList<NotificationModel>> GetNewOrderNotificationList(string Id);
        Task<IList<NotificationModel>> GetNewStatusChangeNotificationList(string Id);
        Task<IList<NotificationModel>> GetNewReplyNotificationList(string Id);
    }
}

