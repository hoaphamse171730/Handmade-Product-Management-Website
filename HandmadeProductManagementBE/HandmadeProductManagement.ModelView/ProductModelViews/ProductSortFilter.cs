namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductSortFilter
    {
        public bool SortByPrice { get; set; }
        public bool SortByRating { get; set; } = true; // Default sorting by Rating
        public bool SortDescending { get; set; } = true; // Default to descending sort
    }
}
