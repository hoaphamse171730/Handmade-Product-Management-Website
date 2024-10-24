using System.Text.Json.Serialization;

namespace HandmadeProductManagement.ModelViews.OrderDetailModelViews;

public class OrderDetailForManipulationDto
{
    public string ProductItemId { get; set; }
    [JsonIgnore]
    public string? OrderId { get; set; }
    public int ProductQuantity { get; set; }
    [JsonIgnore]
    public decimal DiscountPrice { get; set; }
}