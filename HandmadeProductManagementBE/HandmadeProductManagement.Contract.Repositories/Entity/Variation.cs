using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity 
{
    public class Variation : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public Category? Category { get; set; }

        public ICollection<VariationOption> VariationOptions { get; set; } = [];
    }
}

