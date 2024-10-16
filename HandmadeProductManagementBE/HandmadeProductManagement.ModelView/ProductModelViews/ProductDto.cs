using HandmadeProductManagement.ModelViews.VariationModelViews;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public record ProductDto
    {
        public string? Id { get; set; } 
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryId { get; set; } = string.Empty;
        public List<VariationForProductCreationDto> Variations { get; set; } = new List<VariationForProductCreationDto>();
    }
}
