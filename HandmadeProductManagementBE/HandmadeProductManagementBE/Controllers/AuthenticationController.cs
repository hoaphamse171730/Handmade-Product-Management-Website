using System.Web;
using HandmadeProductManagement.Contract.Repositories.Entity;
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
    IAuthenticationService authenticationService
    
)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModelView loginModelView)
    {
        var result = await authenticationService.LoginAsync(loginModelView);
        return result.StatusCode == StatusCodeHelper.Unauthorized ? Unauthorized(result) : Ok(result);

    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<string>>> Register(RegisterModelView registerModelView)
    {
        var result = await authenticationService.RegisterAsync(registerModelView);
        return result.StatusCode == StatusCodeHelper.BadRequest ? BadRequest(result) : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/register")]
    public async Task<ActionResult<BaseResponse<string>>> RegisterForAdmin(RegisterModelView registerModelView)
    {
        var result = await authenticationService.RegisterAdminAsync(registerModelView);
        return result.StatusCode == StatusCodeHelper.BadRequest ? BadRequest(result) : Ok(result);
    }


    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<BaseResponse<string>>> ForgotPassword(ForgotPasswordModelView forgotPasswordModelView)
    {
        var user = await userManager.FindByEmailAsync(forgotPasswordModelView.Email);
        if (user == null
            // || !user.EmailConfirmed
           )
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

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok("This is an Admin only endpoint");
    }

    [Authorize(Roles = "Seller")]
    [HttpGet("seller-only")]
    public IActionResult SellerOnlyEndpoint()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new { Message = "This is a Seller only endpoint", Claims = claims });
    }

    [Authorize]
    [HttpGet("test-claims")]
    public IActionResult TestClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        //var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        //var nameIdentifier = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        //var emailAddress = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        //var role = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;

        return Ok(claims);
    }

}