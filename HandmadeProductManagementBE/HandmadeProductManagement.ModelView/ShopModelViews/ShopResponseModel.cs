namespace HandmadeProductManagement.ModelViews.ShopModelViews
{
    public class ShopResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Rating { get; set; }
        public Guid UserId { get; set; }
    }
}
