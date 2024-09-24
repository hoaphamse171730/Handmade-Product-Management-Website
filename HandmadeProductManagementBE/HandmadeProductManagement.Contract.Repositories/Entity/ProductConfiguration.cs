using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    internal class ProductConfiguration
    {
        public string ProductItemId { get; set; } = string.Empty;
        public ProductItem ProductItem { get; set; } = new ProductItem();

        public string VariationOptionId { get; set; } = string.Empty ;
        public VariationOption VariationOption { get; set; } = new VariationOption();

    }
}
