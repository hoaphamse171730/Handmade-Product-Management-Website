using System.IdentityModel.Tokens.Jwt;
using System.Web;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.Repositories.Entity;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HandmadeProductManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(
    /*UserManager<ApplicationUser> userManager,*/
    /*TokenService tokenService,*/
    /*IEmailService emailService,*/
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
    public async Task<IActionResult> ForgotPassword(ForgotPasswordModelView forgotPasswordModelView)
    {
        var result = await authenticationService.ForgotPasswordAsync(forgotPasswordModelView);
        return result.StatusCode == StatusCodeHelper.BadRequest ? BadRequest(result) : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModelView resetPasswordModelView)
    {
        var result = await authenticationService.ResetPasswordAsync(resetPasswordModelView);
        return result.StatusCode == StatusCodeHelper.BadRequest ? BadRequest(result) : Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordModelView changePasswordModelView)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var result = await authenticationService.ChangePasswordAsync(changePasswordModelView, userId);
        return result.StatusCode == StatusCodeHelper.BadRequest ? BadRequest(result) : Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailModelView confirmEmailModelView)
    {
        var result = await authenticationService.ConfirmEmailAsync(confirmEmailModelView);
        return result.StatusCode == StatusCodeHelper.BadRequest ? BadRequest(result) : Ok(result);
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


    [HttpPost]
    [Route("google-login")]
    public async Task<IActionResult> GoogleLogin(string token)
    {
        var response = await authenticationService.GoogleLoginAsync(token);
        return Ok(new { Token = response.Data });
    }

    [HttpPost]
    [Route("facebook-login")]
    public async Task<IActionResult> FacebookLogin(string token)
    {
        var response = await authenticationService.FacebookLoginAsync(token);
        return Ok(new { Token = response.Data });
    }


}