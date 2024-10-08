

namespace HandmadeProductManagement.ModelViews.VariationModelViews
{
    public class VariationDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public required string CategoryId { get; set; }
    }
}
