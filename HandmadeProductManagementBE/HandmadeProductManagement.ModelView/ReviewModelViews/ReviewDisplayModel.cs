using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.ReviewModelViews
{
    public class ReviewDisplayModel : ReviewModel
    {
        public string UserDisplayName { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
    }
}
