using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;

namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews
{
    public class OrderInDetailDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string ProductName { get; set; }
        public int ProductQuantity { get; set; }
        public decimal DiscountPrice { get; set; }
        public List<string> VariationOptionValues { get; set; } = [];
    }
}
