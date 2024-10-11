using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.CategoryModelViews
{
    public class CategoryForUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
