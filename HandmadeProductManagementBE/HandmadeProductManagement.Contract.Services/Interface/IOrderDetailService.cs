using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IOrderDetailService
{
    Task<IList<OrderDetailDto>> GetAll();
    Task<OrderDetailDto> GetById(string id);
    Task<OrderDetailDto> Create(OrderDetailForCreationDto promotion);
    Task<OrderDetailDto> Update(string orderDetailId, OrderDetailForUpdateDto promotion); 
    Task<bool> SoftDelete(string id);
    //Task<OrderDetailDto> GetByOrderId(string orderId); // cai nay fix sau
}