using HandmadeProductManagement.ModelViews.NotificationModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface INotificationService
    {
        Task<IList<NotificationModel>> GetNewReviewNotificationList(string Id);

        Task<IList<NotificationModel>> GetNewOrderNotificationList(string Id);
        Task<IList<NotificationModel>> GetNewStatusChangeNotificationList(string Id);
        Task<IList<NotificationModel>> GetNewReplyNotificationList(string Id);

    }
}
