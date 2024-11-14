namespace HandmadeProductManagement.ModelViews.ProductDetailModelViews
{
    public class ProductDetailResponseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ShopId { get; set; }
        public string OwnerId { get; set; }
        public string ShopName { get; set; }
        public decimal Rating { get; set; }
        public string Status { get; set; }
        public int SoldCount { get; set; }
        public List<string> ProductImageUrls { get; set; }
        public List<ProductItemDetailModel> ProductItems { get; set; }
        public PromotionDetailModel? Promotion { get; set; }
    }
}