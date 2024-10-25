using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.DashboardModelViews
{
    public class SalesTrendDto
    {
        public List<string> Dates { get; set; }
        public List<decimal> Sales { get; set; }
    }
}
