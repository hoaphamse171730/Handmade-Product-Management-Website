using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;
using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    internal class CartItem : BaseEntity
    {
        public string CartId { get; set; } = string.Empty;
        public Cart Cart { get; set; } = new Cart();

        /*        public String ProductId { get; set; }
                public Product Product { get; set; } = new Product();*/
        public int ProductQuantity { get; set; }
    }
}
