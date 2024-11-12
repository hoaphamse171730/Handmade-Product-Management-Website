using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public record ProductForUpdateNewFormatDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
        public List<VariationForProductUpdateNewFormatDto>? Variations { get; set; }
        public List<VariationCombinationUpdateNewFormatDto>? VariationCombinations { get; set; }
    }
}
