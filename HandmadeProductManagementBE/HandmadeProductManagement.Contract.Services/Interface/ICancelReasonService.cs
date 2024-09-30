using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface ICancelReasonService
    {
        Task<IList<CancelReason>> GetAll();
        Task<CancelReason> GetById(string id);
        Task<CancelReason> Create(CancelReason cancelReason);
        Task<CancelReason> Update(string id, CancelReason cancelReason);
        Task<bool> Delete(string id);
        Task<IList<CancelReason>> GetByPage(int page, int pageSize);
    }
}
