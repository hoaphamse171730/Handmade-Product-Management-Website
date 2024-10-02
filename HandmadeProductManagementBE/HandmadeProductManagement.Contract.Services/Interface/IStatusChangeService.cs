using HandmadeProductManagement.ModelViews.StatusChangeModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IStatusChangeService
    {
        Task<IList<StatusChangeResponseModel>> GetByPage(int page, int pageSize);
        Task<IList<StatusChangeResponseModel>> GetByOrderId(string orderId);
        Task<bool> Create(StatusChangeForCreationDto statusChange);
        Task<bool> Update(string id, StatusChangeForUpdateDto statusChange);
        Task<bool> Delete(string id);
        
    }
}
