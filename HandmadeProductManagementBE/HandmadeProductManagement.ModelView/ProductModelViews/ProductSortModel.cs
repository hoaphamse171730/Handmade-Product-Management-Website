using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductSortModel
    {
        public bool SortByPrice { get; set; } = true;
        public bool SortByRating { get; set; } = false;
        public bool SortDescending { get; set; } = false;
    }
}
