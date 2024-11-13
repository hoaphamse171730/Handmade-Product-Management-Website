namespace HandmadeProductManagement.ModelViews.CartItemModelViews
{
    public class CartItemDto
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public string ProductItemId { get; set; }
        public int ProductQuantity { get; set; }
        public bool InStock { get; set; }
        public int StockQuantity { get; set; }
        public int UnitPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal TotalPriceEachProduct { get; set; }
        public List<string> VariationOptionValues { get; set; }
    }
}
