using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.VariationModelViews
{
    public class VariationForProductUpdateNewFormatResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public List<string>? VariationOptionIds { get; set; }
    }
}
