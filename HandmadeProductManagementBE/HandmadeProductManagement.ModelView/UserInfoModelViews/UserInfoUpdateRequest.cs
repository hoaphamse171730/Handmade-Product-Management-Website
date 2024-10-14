using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.ModelViews.UserInfoModelViews
{
    public class UserInfoUpdateRequest
    {
        public IFormFile? AvtFile { get; set; }
        public UserInfoForUpdateDto? UserInfo { get; set; }
    }
}
