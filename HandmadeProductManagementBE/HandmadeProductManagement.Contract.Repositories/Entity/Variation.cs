using HandmadeProductManagement.Core.Base;
using System.Text.Json.Serialization;

namespace HandmadeProductManagement.Contract.Repositories.Entity 
{
    public class Variation : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        [JsonIgnore]
        public Category? Category { get; set; }
        [JsonIgnore]
        public ICollection<VariationOption> VariationOptions { get; set; } = [];
    }
}

