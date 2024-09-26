using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController(UserManager<ApplicationUser> userManager) 
    : ControllerBase
{
    // private readonly TokenService _tokenService;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserResponseModel>> Register(RegisterModel registerModel)
    {
        if (await userManager.Users.AnyAsync(x => x.UserName == registerModel.UserName))
        {
            ModelState.AddModelError("username", "Username taken");
            return ValidationProblem();
        }

        if (await userManager.Users.AnyAsync(x => x.Email == registerModel.Email))
        {
            ModelState.AddModelError("email", "Email taken");
            return ValidationProblem();
        }
        
        var user = registerModel.Adapt<ApplicationUser>();
            
        var result = await userManager.CreateAsync(user, registerModel.Password);
        
        if (result.Succeeded)
        {
            return Ok(user.Adapt<UserResponseModel>());
        }

        return BadRequest(result.Errors);
    }
}