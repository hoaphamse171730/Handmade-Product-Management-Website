using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class UserInfo : BaseEntity
    {
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? DisplayName { get; set; } = default!;
        [MaxLength(500)]
        public string? Bio { get; set; } = default!;
        [MaxLength(100)]
        public string? BankAccount { get; set; }
        [MaxLength(100)]
        public string? BankAccountName { get; set; }
        [MaxLength(100)]
        public string? Bank { get; set; }
    }
}
