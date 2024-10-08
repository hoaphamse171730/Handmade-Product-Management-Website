using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IOrderDetailService
{
    Task<IList<OrderDetailDto>> GetAll();
    Task<OrderDetailDto> GetById(string id);
    Task<OrderDetailDto> Create(OrderDetailForCreationDto promotion);
}