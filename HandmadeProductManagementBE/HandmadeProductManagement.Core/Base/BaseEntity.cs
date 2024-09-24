using System.ComponentModel.DataAnnotations;
using HandmadeProductManagement.Core.Utils;

namespace HandmadeProductManagement.Core.Base
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid().ToString("N");
            CreatedTime = LastUpdatedTime = CoreHelper.SystemTimeNow;
        }

        [Key]
        public string Id { get; set; }
        public string? CreatedBy { get; set; }
        public string? LastUpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
    }
}
