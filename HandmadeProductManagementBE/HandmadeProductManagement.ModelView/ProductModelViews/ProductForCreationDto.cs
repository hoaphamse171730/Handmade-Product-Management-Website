using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public record ProductForCreationDto
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryId { get; set; } = string.Empty;
        public string? ShopId { get; set; } = string.Empty;
        public List<VariationForProductCreationDto> Variations { get; set; } = new List<VariationForProductCreationDto>();
        public List<VariationCombinationDto> VariationCombinations { get; set; } = new List<VariationCombinationDto>();
    }
}
