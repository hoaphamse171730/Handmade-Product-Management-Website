using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.OrderModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IOrderService
    {
        Task<OrderWithDetailDto> GetOrderByIdAsync(string orderId, string userId, string role);
        Task<bool> CreateOrderAsync(string userId, CreateOrderDto createOrder);
        
        Task<OrderResponseModel> CreateOrderAsyncReturnOrder(string userId, CreateOrderDto createOrder);
        Task<bool> UpdateOrderAsync(string userId, string orderId, UpdateOrderDto order);
        Task<bool> UpdateOrderStatusAsync(string userId, UpdateStatusOrderDto updateStatusOrderDto);
        Task<bool> UpdateOrderStatusToProcessingAsync(string orderId, string userId);
        Task<IList<OrderByUserDto>> GetOrderByUserIdAsync(Guid userId, int pageNumber, int pageSize);
        Task<IList<OrderResponseModel>> GetOrderByUserIdForAdminAsync(Guid userId);
        Task<IList<OrderResponseModel>> GetOrdersByPageAsync(int pageNumber, int pageSize);
        Task<IList<OrderResponseModel>> GetOrdersBySellerUserIdAsync(Guid userId,string filter, int pageNumber, int pageSize);
    }
}
