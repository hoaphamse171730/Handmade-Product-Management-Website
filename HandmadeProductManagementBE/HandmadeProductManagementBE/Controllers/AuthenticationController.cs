using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using HandmadeProductManagement.Services.Service;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(UserManager<ApplicationUser> userManager, TokenService tokenService)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<UserLoginResponseModel>>> Login(LoginModelView loginModelView)
    {
        if (string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) &&
            string.IsNullOrWhiteSpace(loginModelView.Email) &&
            string.IsNullOrWhiteSpace(loginModelView.UserName) ||
            string.IsNullOrWhiteSpace(loginModelView.Password)
           )
        {
            return new BaseResponse<UserLoginResponseModel>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "At least one of Phone Number, Email, or Username is required for login.",
            };
        }

        var user = await userManager.Users
            .Include(u => u.UserInfo)
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Email == loginModelView.Email
                                      || u.PhoneNumber == loginModelView.PhoneNumber
                                      || u.UserName == loginModelView.UserName);

        if (user is null)
        {
            return new BaseResponse<UserLoginResponseModel>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "Incorrect user login credentials"
            };
        }

        if (user.Status != Constants.UserActiveStatus)
        {
            return new BaseResponse<UserLoginResponseModel>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "This account has been disabled."
            };
        }
        
        var success = await userManager.CheckPasswordAsync(user, loginModelView.Password);

        if (success)
        {
            return BaseResponse<UserLoginResponseModel>.OkResponse(CreateUserResponse(user));
        }

        return new BaseResponse<UserLoginResponseModel>()
        {
            StatusCode = StatusCodeHelper.Unauthorized,
            Message = "Incorrect password",
        };
    }

    private UserLoginResponseModel CreateUserResponse(ApplicationUser user)
    {
        return new UserLoginResponseModel()
        {
            FullName = user.UserInfo.FullName,
            UserName = user.UserName,
            DisplayName = user.UserInfo.DisplayName,
            Token = tokenService.CreateToken(user)
        };
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<UserResponseModel>>> Register(RegisterModelView registerModelView)
    {
        if (!ValidationHelper.IsValidNames(CustomRegex.UsernameRegex, registerModelView.UserName) ||
            !ValidationHelper.IsValidNames(CustomRegex.FullNameRegex, registerModelView.FullName)
           )
            return new BaseResponse<UserResponseModel>()
            {
                StatusCode = StatusCodeHelper.Unauthorized,
                Message = "Username or Full Name contains invalid characters.",
            };

        if (await userManager.Users.AnyAsync(x => x.UserName == registerModelView.UserName))
        {
            ModelState.AddModelError("username", "Username is already taken");
        }

        if (await userManager.Users.AnyAsync(x => x.Email == registerModelView.Email))
        {
            ModelState.AddModelError("email", "Email is already taken");
        }

        if (await userManager.Users.AnyAsync(x => x.PhoneNumber == registerModelView.PhoneNumber))
        {
            ModelState.AddModelError("phone", "Phone is already taken");
        }

        //Return validation errors if any
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return new BaseResponse<UserResponseModel>
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "Validation failed: " + string.Join("; ", errors),
                Data = null
            };
        }

        var user = registerModelView.Adapt<ApplicationUser>();

        var result = await userManager.CreateAsync(user, registerModelView.Password);

        if (result.Succeeded)
        {
            return BaseResponse<UserResponseModel>.OkResponse(user.Adapt<UserResponseModel>());
        }

        return new BaseResponse<UserResponseModel>()
        {
            StatusCode = StatusCodeHelper.BadRequest,
            Message = result.Errors.ToString(),
        };
    }
}