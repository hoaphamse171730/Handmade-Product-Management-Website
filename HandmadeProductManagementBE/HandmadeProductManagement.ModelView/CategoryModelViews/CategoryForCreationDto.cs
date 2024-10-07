using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.CategoryModelViews
{
    public class CategoryForCreationDto : CategoryForManipulationDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [JsonIgnore]
        public string? PromotionId { get; set; } = string.Empty;
    }
}
