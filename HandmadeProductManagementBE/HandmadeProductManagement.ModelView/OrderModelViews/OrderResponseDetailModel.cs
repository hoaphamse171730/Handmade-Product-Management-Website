using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class OrderResponseDetailModel : OrderResponseModel
    {
        public List<OrderDetailResponseModel> OrderDetails { get; set; } = new List<OrderDetailResponseModel>();
    }
}
