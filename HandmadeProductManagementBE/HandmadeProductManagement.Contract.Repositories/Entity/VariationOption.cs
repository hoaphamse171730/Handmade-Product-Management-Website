using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity 
{
    public class VariationOption : BaseEntity
    {
        public string Value { get; set; } = string.Empty;
        public string VariationId { get; set; } = string.Empty;
        public Variation? Variation { get; set; }

        public ICollection<ProductConfiguration> ProductConfiguration { get; set; } = new List<ProductConfiguration>();

    }
}

