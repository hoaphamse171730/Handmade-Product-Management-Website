using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ReviewModelViews
{
    public class ReviewForCreationDto
    {
        public string Content { get; set; }
        public required int Rating { get; set; }
        public required string ProductId { get; set; }
        public required string OrderId { get; set; }
    }
}
