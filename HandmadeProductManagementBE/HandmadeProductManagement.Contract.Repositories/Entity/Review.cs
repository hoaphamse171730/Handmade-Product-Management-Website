using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Review : BaseEntity
    {
        public string? Content { get; set; }
        public int Rating { get; set; }
        public DateTime Date { get; set; }

        // Foreign key to the Product entity
        public string ProductId { get; set; } = string.Empty;
        //public Product Product { get; set; } = new Product();


        // Foreign key to the User entity
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } =  new ApplicationUser();


        // One-to-one relationship with Reply
        public Reply? Reply { get; set; }
    }
}
