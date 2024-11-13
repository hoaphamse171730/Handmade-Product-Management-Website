
using HandmadeProductManagement.Core.Common;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductSearchFilter
    {
        public string? Name { get; set; }
        public string? CategoryId { get; set; }
        public string? ShopId { get; set; }
        public string? Status { get; set; }
        public decimal? MinRating { get; set; }

        // Default sorting by Rating Descending
        public string SortOption { get; set; } = Constants.SortByRating;
        public bool SortDescending { get; set; } = true;
    }
}
