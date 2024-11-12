using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductForUpdateNewFormatResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryId { get; set; }

        public List<VariationForProductUpdateNewFormatResponseDto>? Variations { get; set; }
        public List<VariationCombinationUpdateNewFormatDto>? ProductItems { get; set; }
    }
}
