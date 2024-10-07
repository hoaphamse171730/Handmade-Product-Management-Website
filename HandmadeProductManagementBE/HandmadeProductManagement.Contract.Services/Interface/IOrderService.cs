using HandmadeProductManagement.ModelViews.OrderModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IOrderService
    {
        Task<IList<OrderResponseModel>> GetAllOrdersAsync();
        Task<OrderResponseModel> GetOrderByIdAsync(string orderId);
        Task<bool> CreateOrderAsync(CreateOrderDto createOrder, string username);
        Task<bool> UpdateOrderAsync(string orderId, UpdateOrderDto order, string username);
        Task<bool> UpdateOrderStatusAsync(UpdateStatusOrderDto updateStatusOrderDto, string username);
        Task<IList<OrderResponseModel>> GetOrderByUserIdAsync(Guid userId);
    }
}
