using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity;

public class Variation : BaseEntity
{
    public required string Name { get; set; }
    public required string CategoryId { get; set; }
    // public required Category Category { get; set; }
    public ICollection<VariationOption> VariationOptions { get; set; } = [];
}
