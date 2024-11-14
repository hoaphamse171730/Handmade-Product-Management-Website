using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.ModelViews.VariationCombinationModelViews
{
    public class VariationCombinationUpdateNewFormatDto
    {
        public List<string>? VariationOptionIds { get; set; }
        public int? Price { get; set; }
        public int? QuantityInStock { get; set; }
    }
}
