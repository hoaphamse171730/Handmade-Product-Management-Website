using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Reply : BaseEntity
    {
        public string? Content { get; set; }
        public DateTime? Date { get; set; }


        // Foreign key to the Review entity for one-to-one relationship
        public string ReviewId { get; set; } = string.Empty;
        public Review? Review { get; set; }


        // Foreign key to the Shop entity
        public string ShopId { get; set; } = string.Empty;
        public Shop? Shop { get; set; }
    }
}
