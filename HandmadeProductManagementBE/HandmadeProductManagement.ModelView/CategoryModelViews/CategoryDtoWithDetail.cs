using HandmadeProductManagement.Contract.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.CategoryModelViews
{
    public class CategoryDtoWithDetail : CategoryDto
    {
        public Promotion? Promotion { get; set; }
    }
}
