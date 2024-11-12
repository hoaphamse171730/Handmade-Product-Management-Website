using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.VariationModelViews
{
    public class VariationForProductUpdateNewFormatDto
    {
        public string? Id { get; set; }
        public List<string>? VariationOptionIds { get; set; }
    }
}
