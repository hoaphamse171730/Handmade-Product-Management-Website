using HandmadeProductManagement.Core.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class UserInfoImage : BaseEntity
    {
        public string Url { get; set; } = string.Empty;
     
        public UserInfo UserInfo { get; set; } = new UserInfo();
    }
}
