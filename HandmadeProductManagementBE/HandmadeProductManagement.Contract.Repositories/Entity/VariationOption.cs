using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity;

public class VariationOption : BaseEntity
{
    public required string Value { get; set; }
    public required string VariationId { get; set; }
    public required Variation Variation { get; set; }
}