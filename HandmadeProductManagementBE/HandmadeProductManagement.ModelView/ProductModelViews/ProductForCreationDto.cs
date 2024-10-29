using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public record ProductForCreationDto
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public string CategoryId { get; set; }
        [JsonIgnore]
        public string? ShopId { get; set; } = string.Empty;
        public List<VariationForProductCreationDto> Variations { get; set; } = [];
        public List<VariationCombinationDto> VariationCombinations { get; set; } = [];
    }
}
