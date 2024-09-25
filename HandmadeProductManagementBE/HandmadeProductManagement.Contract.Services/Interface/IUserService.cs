using HandmadeProductManagement.ModelViews.UserModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IUserService
    {
        Task<IList<UserResponseModel>> GetAll();
        Task<UserResponseModel> GetUserByCredentials(string username, string password);


        Task<string> LoginUser(string username, string password);
    }
}

