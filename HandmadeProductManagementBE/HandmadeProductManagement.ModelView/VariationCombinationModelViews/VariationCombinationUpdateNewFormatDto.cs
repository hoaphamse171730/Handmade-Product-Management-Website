using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.ModelViews.VariationCombinationModelViews
{
    public class VariationCombinationUpdateNewFormatDto
    {
        public string ProductItemId { get; set; } = string.Empty;
        public List<OptionsDto>? Combinations { get; set; }
        public int? Price { get; set; }
        public int? QuantityInStock { get; set; }
    }
}
