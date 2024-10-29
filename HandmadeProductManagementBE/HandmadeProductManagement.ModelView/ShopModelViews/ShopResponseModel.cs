namespace HandmadeProductManagement.ModelViews.ShopModelViews
{
    public class ShopResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Rating { get; set; }
        public int ProductCount { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string JoinedTimeAgo
        {
            get
            {
                var now = DateTime.Now;
                var difference = now - CreatedTime;

                if (difference.TotalDays >= 365)
                {
                    int years = (int)(difference.TotalDays / 365);
                    return $"{years} year{(years > 1 ? "s" : "")} ago";
                }
                else if (difference.TotalDays >= 30)
                {
                    int months = (int)(difference.TotalDays / 30);
                    return $"{months} month{(months > 1 ? "s" : "")} ago";
                }
                else
                {
                    int days = (int)difference.TotalDays;
                    return $"{days} day{(days > 1 ? "s" : "")} ago";
                }
            }
        }
        public Guid UserId { get; set; }
    }
}
