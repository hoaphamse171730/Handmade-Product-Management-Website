using Microsoft.AspNetCore.Http;

namespace HandmadeProductManagement.ModelViews.UserInfoModelViews
{
    public class UserInfoUpdateRequest
    {
        public UserInfoForUpdateDto? UserInfo { get; set; }
    }
}
