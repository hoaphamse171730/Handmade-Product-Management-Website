using HandmadeProductManagement.ModelViews.OrderModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IOrderService
    {
        Task<IList<OrderResponseModel>> GetAllOrdersAsync();
        Task<OrderResponseModel> GetOrderByIdAsync(string orderId);
        Task<bool> CreateOrderAsync(CreateOrderDto createOrder);
        Task<bool> UpdateOrderAsync(string orderId, UpdateOrderDto order);
        Task<bool> UpdateOrderStatusAsync(UpdateStatusOrderDto updateStatusOrderDto);
        Task<IList<OrderResponseModel>> GetOrderByUserIdAsync(Guid userId);
    }
}
