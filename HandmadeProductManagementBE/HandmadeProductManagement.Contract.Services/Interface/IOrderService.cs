using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderResponseModel> GetOrderByIdAsync(string orderId, string userId);
        Task<bool> CreateOrderAsync(string userId, CreateOrderDto createOrder);
        Task<bool> UpdateOrderAsync(string userId, string orderId, UpdateOrderDto order);
        Task<bool> UpdateOrderStatusAsync(string userId, UpdateStatusOrderDto updateStatusOrderDto);
        Task<IList<OrderResponseModel>> GetOrderByUserIdAsync(Guid userId);
        Task<PaginatedList<OrderResponseModel>> GetOrdersByPageAsync(int pageNumber, int pageSize);
        Task<IList<OrderResponseModel>> GetOrdersBySellerUserIdAsync(Guid userId);
    }
}
