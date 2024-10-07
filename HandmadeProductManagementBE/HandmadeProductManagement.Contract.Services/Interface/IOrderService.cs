using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderResponseModel> GetOrderByIdAsync(string orderId);
        Task<bool> CreateOrderAsync(string userId, CreateOrderDto createOrder, string username);
        Task<bool> UpdateOrderAsync(string orderId, UpdateOrderDto order, string username);
        Task<bool> UpdateOrderStatusAsync(UpdateStatusOrderDto updateStatusOrderDto, string username);
        Task<IList<OrderResponseModel>> GetOrderByUserIdAsync(Guid userId);
        Task<PaginatedList<OrderResponseModel>> GetOrdersByPageAsync(int pageNumber, int pageSize);
    }
}
