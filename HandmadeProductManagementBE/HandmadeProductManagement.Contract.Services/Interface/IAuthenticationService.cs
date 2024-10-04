using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IAuthenticationService
{
    Task<UserLoginResponseModel> AuthenticateUser(LoginModelView loginModelView);
    Task<UserLoginResponseModel> CreateUserResponse(ApplicationUser user);
    Task AssignRoleToUser(string userId, string role);
}