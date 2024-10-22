namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductSearchFilter
    {
        public string? Name { get; set; }
        public string? CategoryId { get; set; }
        public string? ShopId { get; set; }
        public string? Status { get; set; }
        public decimal? MinRating { get; set; }

        public bool SortByPrice { get; set; }
        public bool SortByRating { get; set; } = true; // Default sorting by Rating
        public bool SortDescending { get; set; } = true; // Default to descending sort


    }
}
