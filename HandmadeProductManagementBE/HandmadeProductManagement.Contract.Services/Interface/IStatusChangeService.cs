using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IStatusChangeService
    {
        Task<IList<StatusChange>> GetAll();
        Task<IList<StatusChange>> GetByOrderId(string orderId);
        Task<StatusChange> GetById(string id);
        Task<StatusChange> Create(StatusChange statusChange);
        Task<StatusChange> Update(string id, StatusChange statusChange);
        Task<bool> Delete(string id);
        Task<IList<StatusChange>> GetByPage(int page, int pageSize);
    }
}
