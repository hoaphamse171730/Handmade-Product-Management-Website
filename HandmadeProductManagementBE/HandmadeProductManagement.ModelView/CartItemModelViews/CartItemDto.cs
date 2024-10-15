namespace HandmadeProductManagement.ModelViews.CartItemModelViews
{
    public class CartItemDto
    {
        public string Id { get; set; }
        public string ProductItemId { get; set; }
        public int ProductQuantity { get; set; }
        public int UnitPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public decimal TotalPriceEachProduct { get; set; }
        public List<string> VariationOptionValues { get; set; }
    }
}
