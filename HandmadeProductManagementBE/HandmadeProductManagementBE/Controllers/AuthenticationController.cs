using System.Web;
using HandmadeProductManagement.Contract.Services.Interface;
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
public class AuthenticationController(
    UserManager<ApplicationUser> userManager,
    TokenService tokenService,
    IEmailService emailService,
    AuthenticationService authenticationService
    ) : ControllerBase
{
    // [AllowAnonymous]
    // [HttpPost("login")]
    // public async Task<ActionResult<BaseResponse<UserLoginResponseModel>>> Login(LoginModelView loginModelView)
    // {
    //     if (string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) &&
    //         string.IsNullOrWhiteSpace(loginModelView.Email) &&
    //         string.IsNullOrWhiteSpace(loginModelView.UserName)
    //        )
    //     {
    //         return new BaseResponse<UserLoginResponseModel>()
    //         {
    //             StatusCode = StatusCodeHelper.Unauthorized,
    //             Message = "At least one of Phone Number, Email, or Username is required for login.",
    //         };
    //     }
    //
    //     var user = await userManager.Users
    //         .Include(u => u.UserInfo)
    //         .Include(u => u.Cart)
    //         .FirstOrDefaultAsync(u => u.Email == loginModelView.Email
    //                                   || u.PhoneNumber == loginModelView.PhoneNumber
    //                                   || u.UserName == loginModelView.UserName);
    //
    //     if (user is null)
    //     {
    //         return new BaseResponse<UserLoginResponseModel>()
    //         {
    //             StatusCode = StatusCodeHelper.Unauthorized,
    //             Message = "Incorrect user login credentials"
    //         };
    //     }
    //
    //     if (user.Status != Constants.UserActiveStatus)
    //     {
    //         return new BaseResponse<UserLoginResponseModel>()
    //         {
    //             StatusCode = StatusCodeHelper.Unauthorized,
    //             Message = "This account has been disabled."
    //         };
    //     }
    //
    //     var success = await userManager.CheckPasswordAsync(user, loginModelView.Password);
    //
    //     if (success)
    //     {
    //         return BaseResponse<UserLoginResponseModel>.OkResponse(CreateUserResponse(user));
    //     }
    //
    //     return new BaseResponse<UserLoginResponseModel>()
    //     {
    //         StatusCode = StatusCodeHelper.Unauthorized,
    //         Message = "Incorrect password",
    //     };
    // }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModelView loginModelView)
    {
        var result = await authenticationService.AuthenticateUser(loginModelView);
        return Ok(BaseResponse<UserLoginResponseModel>.OkResponse(result));
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
    public async Task<ActionResult<BaseResponse<string>>> Register(RegisterModelView registerModelView)
    {
        if (!ValidationHelper.IsValidNames(CustomRegex.UsernameRegex, registerModelView.UserName) ||
            !ValidationHelper.IsValidNames(CustomRegex.FullNameRegex, registerModelView.FullName)
           )
            return new BaseResponse<string>()
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

            return new BaseResponse<string>
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
            await emailService.SendEmailConfirmationAsync(user.Email!, registerModelView.ClientUri);
            return BaseResponse<string>.OkResponse(user.Id.ToString());
        }

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.BadRequest,
            Message = string.Join(", ", result.Errors)
        };
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<BaseResponse<string>>> ForgotPassword(ForgotPasswordModelView forgotPasswordModelView)
    {
        var user = await userManager.FindByEmailAsync(forgotPasswordModelView.Email);
        if (user == null || !user.EmailConfirmed)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "Email is invalid or not confirmed."
            };
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(resetToken); //encode the token for URL safety

        var passwordResetLink = $"{forgotPasswordModelView.ClientUri}?email={user.Email}&token={encodedToken}";

        await emailService.SendPasswordRecoveryEmailAsync(user.Email!, passwordResetLink);

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.OK,
            Message = "Password reset link has been sent to your email."
        };
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<BaseResponse<string>>> ResetPassword(ResetPasswordModelView resetPasswordModelView)
    {
        var user = await userManager.FindByEmailAsync(resetPasswordModelView.Email);
        if (user == null)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "Invalid request."
            };
        }

        var decodedToken = HttpUtility.UrlDecode(resetPasswordModelView.Token); //decode the token from the request
        var result = await userManager.ResetPasswordAsync(user, decodedToken, resetPasswordModelView.NewPassword);

        if (result.Succeeded)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.OK,
                Message = "Password has been reset successfully."
            };
        }

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.BadRequest,
            Message = "Error resetting the password.",
        };
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<BaseResponse<string>> ConfirmEmail(ConfirmEmailModelView confirmEmailModelView)
    {
        var user = await userManager.FindByEmailAsync(confirmEmailModelView.Email);
        if (user is null)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.BadRequest,
                Message = "User not found."
            };
        }

        var decodedToken = HttpUtility.UrlDecode(confirmEmailModelView.Token);
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.OK,
                Message = StatusCodeHelper.OK.Name()
            };
        }

        return BaseResponse<string>.FailResponse(statusCode: StatusCodeHelper.BadRequest,
            message: "Error confirming the email.");
    }
}