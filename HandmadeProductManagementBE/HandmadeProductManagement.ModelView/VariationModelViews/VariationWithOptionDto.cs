using HandmadeProductManagement.ModelViews.VariationOptionModelViews;

public class VariationWithOptionsDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public IList<VariationOptionDto> Options { get; set; }
}