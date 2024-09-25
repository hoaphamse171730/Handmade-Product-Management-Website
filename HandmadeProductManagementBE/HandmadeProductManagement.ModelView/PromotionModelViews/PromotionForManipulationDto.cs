using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.ModelViews.PromotionModelViews;

public class PromotionForManipulationDto : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } 
    public string PromotionName { get; set; } = string.Empty;
    public float DiscountRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Status { get; set; }
}