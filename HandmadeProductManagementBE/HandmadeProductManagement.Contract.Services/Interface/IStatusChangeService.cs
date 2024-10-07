using HandmadeProductManagement.ModelViews.StatusChangeModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IStatusChangeService
    {
        Task<IList<StatusChangeDto>> GetByPage(int page, int pageSize);
        Task<IList<StatusChangeDto>> GetByOrderId(string orderId);
        Task<bool> Create(StatusChangeForCreationDto statusChange, string userId);
        //Task<bool> Update(string id, StatusChangeForUpdateDto statusChange, string userId);
        //Task<bool> Delete(string id, string userId);

    }
}
