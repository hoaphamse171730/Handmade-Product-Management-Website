using HandmadeProductManagement.ModelViews.ProductConfigurationModelViews;

namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews
{
    public class OrderDetailResponseModel 
    {
        public string Id { get; set; } = string.Empty;
        public string ProductName { get; set; }
        public int ProductQuantity { get; set; }
        public decimal Price { get; set; }
        public string ProductItemId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public List<ProductConfigResponseModel> ProductConfiguration { get; set; }
    }
}
