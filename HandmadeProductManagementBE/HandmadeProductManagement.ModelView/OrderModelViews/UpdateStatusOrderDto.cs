using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.OrderModelViews
{
    public class UpdateStatusOrderDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status {get; set; } = string.Empty;
        public string? CancelReasonId {get; set; }
    }
}
