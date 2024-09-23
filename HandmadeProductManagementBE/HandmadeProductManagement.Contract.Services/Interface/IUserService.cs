using HandmadeProductManagement.ModelViews.UserModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IUserService
    {
        Task<IList<UserResponseModel>> GetAll();
    }
}

