using HandmadeProductManagement.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class StatusChange : BaseEntity
    {
        public DateTime ChangeTime { get; set; }
        public string Status { get; set; } = string.Empty;  
        public string OrderId { get; set; } = string.Empty;
        public virtual Order? Order { get; set; }
    }
}
