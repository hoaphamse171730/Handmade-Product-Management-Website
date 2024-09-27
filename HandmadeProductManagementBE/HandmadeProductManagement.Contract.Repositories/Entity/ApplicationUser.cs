using Microsoft.AspNetCore.Identity;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagement.Repositories.Entity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public virtual UserInfo UserInfo { get; set; } = new();
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset LastUpdatedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }

        public string status { get; set; } = "active";

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ApplicationUser()
        {
            CreatedTime = CoreHelper.SystemTimeNow;
            LastUpdatedTime = CreatedTime;
        }
        public Shop? Shop { get; set; }
        public ICollection<Order> Orders { get; set; } = [];
    }
}
