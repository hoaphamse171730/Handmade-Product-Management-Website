using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModelView model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var token = await _userService.LoginUser(model.Username, model.Password);

        if (token == null)
            return Unauthorized("Invalid credentials");

        return Ok(new { Token = token });
    }
}

 
 
