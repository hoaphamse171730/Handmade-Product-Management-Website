﻿using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class Shop : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Rating { get; set; }
        public Guid UserId { get; set; }
        [JsonIgnore]
        public ApplicationUser? User { get; set; }

        public virtual ICollection<Product> Products{ get; set; } = new List<Product>();
    }
}
