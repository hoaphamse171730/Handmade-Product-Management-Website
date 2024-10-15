using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.ModelViews.PromotionModelViews;

public class PromotionDto 
{ 
    public string? Id { get; set; }
    public string? Name { get; set; } 
    public string? Description { get; set; } 
    public decimal DiscountRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
}