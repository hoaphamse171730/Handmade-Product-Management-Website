using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Review : BaseEntity
    {
        public string? Content { get; set; }
        public int? Rating { get; set; }
        public DateTime Date { get; set; }

        // Foreign key to the Product entity
        public string ProductId { get; set; } = string.Empty;
        public Product Product { get; set; }


        // Foreign key to the User entity
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }


        // One-to-one relationship with Reply
        public Reply? Reply { get; set; }
    }
}
