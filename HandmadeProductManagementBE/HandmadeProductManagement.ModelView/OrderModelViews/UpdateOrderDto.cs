﻿

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class UpdateOrderDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
