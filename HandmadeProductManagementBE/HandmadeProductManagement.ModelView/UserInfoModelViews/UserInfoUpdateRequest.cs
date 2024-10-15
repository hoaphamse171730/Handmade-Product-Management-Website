using Microsoft.AspNetCore.Http;

namespace HandmadeProductManagement.ModelViews.UserInfoModelViews
{
    public class UserInfoUpdateRequest
    {
        public IFormFile? AvtFile { get; set; }
        public UserInfoForUpdateDto? UserInfo { get; set; }
    }
}
