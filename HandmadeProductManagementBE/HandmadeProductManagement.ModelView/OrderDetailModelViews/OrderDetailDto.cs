using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews;

public class OrderDetailDto
{
    public string? Id { get; set; }
    public string? ProductItemId { get; set; }
    public string? OrderId { get; set; }
    public int ProductQuantity { get; set; } 
    public decimal DiscountPrice { get; set; }
    public string? CreatedBy { get; set; }
    public string? LastUpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset LastUpdatedTime { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
}