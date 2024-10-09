namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public class ProductWithVariationDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public List<VariationWithOptionsDto> Variations { get; set; }
        public List<ProductItemWithDetailsDto> ProductItems { get; set; }
    }
}
