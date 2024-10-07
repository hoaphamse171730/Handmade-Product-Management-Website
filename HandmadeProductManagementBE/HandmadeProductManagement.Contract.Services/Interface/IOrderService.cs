using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderResponseModel> GetOrderByIdAsync(string orderId);
        Task<bool> CreateOrderAsync(string userId, CreateOrderDto createOrder);
        Task<bool> UpdateOrderAsync(string userId, string orderId, UpdateOrderDto order);
        Task<bool> UpdateOrderStatusAsync(UpdateStatusOrderDto updateStatusOrderDto);
        Task<IList<OrderResponseModel>> GetOrderByUserIdAsync(Guid userId);
        Task<PaginatedList<OrderResponseModel>> GetOrdersByPageAsync(int pageNumber, int pageSize);
    }
}
