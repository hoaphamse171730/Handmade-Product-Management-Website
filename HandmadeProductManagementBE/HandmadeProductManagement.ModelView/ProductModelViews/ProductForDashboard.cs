namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductForDashboard
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty; 
        public int Price { get; set; }
        public ICollection<string> ImageUrls { get; set; } = [];
    }
}
