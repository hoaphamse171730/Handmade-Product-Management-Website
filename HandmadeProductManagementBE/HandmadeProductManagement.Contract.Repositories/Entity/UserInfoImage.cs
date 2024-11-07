using HandmadeProductManagement.Core.Base;

namespace HandmadeProductManagement.Contract.Repositories.Entity
{
    public class UserInfoImage : BaseEntity
    {
        public string Url { get; set; } = string.Empty;

        public string UserInfoId { get; set; } = string.Empty;

        public UserInfo UserInfo { get; set; } = new UserInfo();
    }
}
