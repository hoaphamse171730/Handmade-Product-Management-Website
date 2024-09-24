﻿using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<Variation> Variations { get; set; }= [];

        public string PromotionId { get; set; } = string.Empty;
        public virtual Promotion? Promotion { get; set; }
        
        
        
    }
}
