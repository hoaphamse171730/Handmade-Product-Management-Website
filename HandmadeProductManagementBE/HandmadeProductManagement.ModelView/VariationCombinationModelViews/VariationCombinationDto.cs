namespace HandmadeProductManagement.ModelViews.VariationCombinationModelViews
{
    public class VariationCombinationDto
    {
        public List<string> VariationOptionIds { get; set; } = new List<string>();
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
    }
}
