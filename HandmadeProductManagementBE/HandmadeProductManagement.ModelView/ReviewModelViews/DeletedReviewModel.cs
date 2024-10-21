using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ReviewModelViews
{
    public class DeletedReviewModel
    {
        public string Id { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTimeOffset DeletedTime { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
