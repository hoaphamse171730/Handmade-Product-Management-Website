﻿using HandmadeProductManagement.ModelViews.VariationCombinationModelViews;
using HandmadeProductManagement.ModelViews.VariationModelViews;
using System.Text.Json.Serialization;

namespace HandmadeProductManagement.ModelViews.ProductModelViews
{
    public record ProductForCreationDto
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryId { get; set; } = string.Empty;
        [JsonIgnore]
        public string? ShopId { get; set; } = string.Empty;
        public List<VariationForProductCreationDto> Variations { get; set; } = [];
        public List<VariationCombinationDto> VariationCombinations { get; set; } = [];
    }
}
