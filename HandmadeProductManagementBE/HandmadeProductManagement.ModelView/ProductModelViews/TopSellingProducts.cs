namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class TopSellingProducts
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Price { get; set; }
        public string ImageUrl { get; set; }

        public int SoldCount { get; set; }
    }
}
