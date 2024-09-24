using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Category : BaseEntity
    {
        public required string CategoryId { get; set; } 
        public required string CategoryName { get; set; } 
        public string? CategoryDescription { get; set; }

        public required string PromotionId { get; set; } 
        public virtual Promotion? Promotion { get; set; }
        
        
        
    }
}
