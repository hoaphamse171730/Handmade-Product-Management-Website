using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

namespace HandmadeProductManagement.ModelViews.VariationModelViews
{
    public class VariationWithOptionsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<OptionsDto>? Options { get; set; }
    }
}
