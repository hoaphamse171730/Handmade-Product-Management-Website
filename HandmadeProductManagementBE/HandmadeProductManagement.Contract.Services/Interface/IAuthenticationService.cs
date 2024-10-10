using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;

namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IAuthenticationService
{
    Task<BaseResponse<UserLoginResponseModel>> LoginAsync(LoginModelView loginModelView);
    Task<BaseResponse<string>> RegisterAsync(RegisterModelView registerModelView);
    Task<BaseResponse<string>> RegisterAdminAsync(RegisterModelView registerModelView);
}