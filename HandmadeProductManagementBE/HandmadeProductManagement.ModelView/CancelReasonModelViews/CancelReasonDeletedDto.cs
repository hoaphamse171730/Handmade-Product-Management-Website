using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.CancelReasonModelViews
{
    public class CancelReasonDeletedDto
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public decimal RefundRate { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
    }
}
