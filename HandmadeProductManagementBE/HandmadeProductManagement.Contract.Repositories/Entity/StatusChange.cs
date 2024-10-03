using HandmadeProductManagement.Core.Base;
using System.Text.Json.Serialization;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class StatusChange : BaseEntity
    {
        public DateTime ChangeTime { get; set; }
        public string Status { get; set; } = string.Empty;  
        public string OrderId { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual Order? Order { get; set; }
    }
}
