namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public record ProductForManipulationDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryId { get; set; }
    }
}
