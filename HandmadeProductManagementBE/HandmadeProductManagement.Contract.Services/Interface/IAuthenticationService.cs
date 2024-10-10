using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;

namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IAuthenticationService
{
    Task<BaseResponse<UserLoginResponseModel>> LoginAsync(LoginModelView loginModelView);
    Task<bool> AssignRoleToUser(string userId, string role);
}