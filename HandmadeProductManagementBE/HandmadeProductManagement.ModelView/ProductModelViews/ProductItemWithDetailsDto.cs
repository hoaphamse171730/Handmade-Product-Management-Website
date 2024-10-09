using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductItemWithDetailsDto
    {
        public string Id { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public List<VariationOptionDto> VariationOptions { get; set; }
    }
}
