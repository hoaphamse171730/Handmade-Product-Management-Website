using HandmadeProductManagement.ModelViews.StatusChangeModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IStatusChangeService
    {
        Task<StatusChangeResponseModel> Create(CreateStatusChangeDto statusChange);
        Task<StatusChangeResponseModel> Update(string id, CreateStatusChangeDto statusChange);
        Task<bool> Delete(string id);
        Task<IList<StatusChangeResponseModel>> GetByPage(int page, int pageSize);
        Task<IList<StatusChangeResponseModel>> GetByOrderId(string orderId);
    }
}
