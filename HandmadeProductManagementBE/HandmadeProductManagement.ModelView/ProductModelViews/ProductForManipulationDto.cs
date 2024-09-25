using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.ModelViews.ProductModelViews;

public class ProductForManipulationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SoldCount { get; set; }
}