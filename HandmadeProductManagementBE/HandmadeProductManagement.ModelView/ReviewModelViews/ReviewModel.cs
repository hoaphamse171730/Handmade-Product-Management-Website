using HandmadeProductManagement.ModelViews.ReplyModelViews;

namespace HandmadeProductManagement.ModelViews.ReviewModelViews
{
    public class ReviewModel
    {
        public string Id { get; set; }
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTime? Date { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public Guid UserId { get; set; }

        public ReplyModel? Reply { get; set; }
    }
}
