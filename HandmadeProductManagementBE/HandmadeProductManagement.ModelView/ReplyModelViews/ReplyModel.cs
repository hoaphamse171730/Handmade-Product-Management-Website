namespace HandmadeProductManagement.ModelViews.ReplyModelViews
{
    public class ReplyModel
    {
        public string Id { get; set; }
        public string? Content { get; set; }
        public DateTime? Date { get; set; }
        public string ReviewId { get; set; } = string.Empty;
        public string ShopId { get; set; } = string.Empty;
    }
}
