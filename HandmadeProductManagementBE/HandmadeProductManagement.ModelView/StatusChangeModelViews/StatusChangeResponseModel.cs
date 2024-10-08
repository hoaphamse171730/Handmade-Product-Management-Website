﻿namespace HandmadeProductManagement.ModelViews.StatusChangeModelViews
{
    public class StatusChangeResponseModel
    {
        public string Id { get; set; } = string.Empty;
        public required DateTime ChangeTime { get; set; }
        public required string Status { get; set; }
        public required string OrderId { get; set; }
    }
}
