namespace HandmadeProductManagement.ModelViews.CartItemModelViews
{
    public class CartItemGroupDto
    {
        public string ShopId { get; set; }
        public string ShopName { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
    }

}
