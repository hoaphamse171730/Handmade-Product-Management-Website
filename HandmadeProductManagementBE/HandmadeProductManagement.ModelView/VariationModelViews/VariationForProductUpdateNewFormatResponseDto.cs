namespace HandmadeProductManagement.ModelViews.VariationModelViews
{
    public class VariationForProductUpdateNewFormatResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; }
        public List<string>? VariationOptionIds { get; set; }
    }
}
