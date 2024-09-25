namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews;

public class OrderDetailForManipulationDto
{
    public string ProductId { get; set; }
    public string OrderId { get; set; }
    public int ProductQuantity { get; set; }
    public float UnitPrice { get; set; }
}