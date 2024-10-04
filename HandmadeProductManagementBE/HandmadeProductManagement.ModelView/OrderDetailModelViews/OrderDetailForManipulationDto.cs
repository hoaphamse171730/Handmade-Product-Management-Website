namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews;

public class OrderDetailForManipulationDto
{
    public string ProductItemId { get; set; }
    public string? OrderId { get; set; }
    public int ProductQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}