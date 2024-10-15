namespace HandmadeProductManagement.ModelViews.CartItemModelViews
{
    public class CartItemDto
    {
        public string Id { get; set; }
        public string ProductItemId { get; set; }
        public int ProductQuantity { get; set; }
        public int UnitPrice { get; set; }
        public int DiscountPrice { get; set; }
        public int TotalPriceEachProduct { get; set; }
        public Guid UserId { get; set; }
        public string ShopId { get; set; }
        public string ShopName { get; set; }
    }
}
