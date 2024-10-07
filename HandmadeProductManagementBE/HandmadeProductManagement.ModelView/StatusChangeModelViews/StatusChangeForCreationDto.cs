using System.Text.Json.Serialization;

namespace HandmadeProductManagement.ModelViews.StatusChangeModelViews
{
    public class StatusChangeForCreationDto
    {
        public required string Status { get; set; }
        public required string OrderId { get; set; }
    }
}
