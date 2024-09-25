using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductSortModel
    {
        public bool SortByPrice { get; set; }
        public bool SortByRating { get; set; } = true; // Default sorting by Rating
        public bool SortDescending { get; set; } = true; // Default to descending sort
    }
}
