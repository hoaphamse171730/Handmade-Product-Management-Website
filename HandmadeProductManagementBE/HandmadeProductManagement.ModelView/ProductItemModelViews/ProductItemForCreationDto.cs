using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.ModelViews.ProductItemModelViews
{
    public class ProductItemForCreationDto
    {
        public required int QuantityInStock { get; set; }
        public required int Price { get; set; }
        public List<string> VariationOptionIds { get; set; } = new();
    }
}
