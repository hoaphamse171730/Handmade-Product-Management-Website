using HandmadeProductManagement.ModelViews.OrderDetailModelViews;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class OrderResponseDetailForUserModel : OrderByUserDto
    {
        public List<OrderDetailResponseModel> OrderDetails { get; set; } = new List<OrderDetailResponseModel>();
    }
}
