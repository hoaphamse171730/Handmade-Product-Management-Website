
namespace HandmadeProductManagement.ModelViews.VariationOptionModelViews
{
    public class VariationOptionForUpdateDto
    {
        public required string Value { get; set; }
        public required int QuantityInStock { get; set; }
        public required int Price { get; set; }
    }
}
