using HandmadeProductManagement.Contract.Repositories.Entity;
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
    // private readonly TokenService _tokenService;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<UserLoginResponseModel>> Login(LoginModelView loginModelView)
    {
        if (string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) && 
            string.IsNullOrWhiteSpace(loginModelView.Email) && 
            string.IsNullOrWhiteSpace(loginModelView.UserName))
        {
            return Unauthorized("At least one of Phone Number, Email, or Username is required for login.");
        }
        
        var user = await userManager.Users
            .Include(u => u.UserInfo)
            .FirstOrDefaultAsync(u => u.Email == loginModelView.Email
                                      || u.PhoneNumber == loginModelView.PhoneNumber
                                      || u.UserName == loginModelView.UserName);

        if (user is null) return Unauthorized("Incorrect user login credentials");
        var result = await userManager.CheckPasswordAsync(user, loginModelView.Password);
        if (result)
        {
            return CreateUserResponse(user);
        }

        return Unauthorized("Incorrect password");
    }

    private UserLoginResponseModel CreateUserResponse(ApplicationUser user)
    {
        return new UserLoginResponseModel()
        {
            FullName = user.UserInfo.FullName,
            UserName = user.UserName ?? UsernameHelper.GenerateUsername(user.UserInfo.FullName),
            DisplayName = user.UserInfo.DisplayName,
            Token = tokenService.CreateToken(user)
        };
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserResponseModel>> Register(RegisterModelView registerModelView)
    {
        if (await userManager.Users.AnyAsync(x => x.UserName == registerModelView.UserName))
        {
            ModelState.AddModelError("username", "Username taken");
            return ValidationProblem();
        }

        if (await userManager.Users.AnyAsync(x => x.Email == registerModelView.Email))
        {
            ModelState.AddModelError("email", "Email taken");
            return ValidationProblem();
        }
        
        var user = registerModelView.Adapt<ApplicationUser>();
        
        var result = await userManager.CreateAsync(user, registerModelView.Password);

        if (result.Succeeded)
        {
            return Ok(user.Adapt<UserResponseModel>());
        }
        return BadRequest(result.Errors);
    }
}