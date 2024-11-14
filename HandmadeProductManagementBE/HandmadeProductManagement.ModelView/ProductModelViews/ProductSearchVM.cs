namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductSearchVM
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
        public string? ProductImageUrl { get; set; }
        public int LowestPrice { get; set; }
        public string? ShopId { get; set; }
        public decimal Rating { get; set; }
        public string? Status { get; set; }
        public int SoldCount { get; set; }
        public DateTimeOffset CreatedTime { get; set; }

    }
}