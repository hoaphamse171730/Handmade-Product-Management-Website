namespace HandmadeProductManagement.ModelViews.ProductItemModelViews
{
    public class ProductItemDto
    {
        public required string Id { get; set; }
        public required int QuantityInStock { get; set; }
        public required int Price { get; set; }
        public required string ProductId { get; set; }
    }
}
