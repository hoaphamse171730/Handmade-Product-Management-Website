using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;
using System.ComponentModel.DataAnnotations;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class CartItem : BaseEntity
    {
        public string CartId { get; set; } = string.Empty;
        public Cart Cart { get; set; } = new Cart();

        public string ProductItemId { get; set; } = string.Empty;
        public ProductItem ProductItem { get; set; } = new ProductItem();
        public int ProductQuantity { get; set; }
    }
}
