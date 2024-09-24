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
        public required DateTime ChangeTime { get; set; }
        public required String Status { get; set; }

        public required string OrderId { get; set; }
        // public Order Order { get; set; }
    }
}
