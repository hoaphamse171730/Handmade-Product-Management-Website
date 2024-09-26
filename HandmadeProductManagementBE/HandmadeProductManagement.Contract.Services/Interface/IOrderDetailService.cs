using HandmadeProductManagement.Contract.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IOrderDetailService
{
    Task<IList<OrderDetail>> GetAll();
    Task<OrderDetail> GetById(string id);
    Task<OrderDetail> Create(OrderDetail orderDetail);
    Task<OrderDetail> Update(string id, OrderDetail orderDetail);
    Task<bool> Delete(string id);
   
}