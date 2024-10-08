namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class ProductConfiguration
    {
        public string ProductItemId { get; set; } = string.Empty;
        public ProductItem ProductItem { get; set; }

        public string VariationOptionId { get; set; } = string.Empty ;
        public VariationOption VariationOption { get; set; }

    }
}
