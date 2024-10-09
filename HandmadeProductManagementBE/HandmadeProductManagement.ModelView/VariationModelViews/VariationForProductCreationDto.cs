using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.ModelViews.VariationModelViews
{
    public class VariationForProductCreationDto
    {
        public required string Id { get; set; }
        public List<string> VariationOptionIds { get; set; } = new List<string>();
    }
}
