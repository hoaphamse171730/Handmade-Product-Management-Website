using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthenticationService(UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<UserLoginResponseModel> AuthenticateUser(LoginModelView loginModelView)
    {
        if (string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) &&
            string.IsNullOrWhiteSpace(loginModelView.Email) &&
            string.IsNullOrWhiteSpace(loginModelView.UserName))
        {
            throw new BaseException.BadRequestException("missing_login_identifier",
                "At least one of Phone Number, Email, or Username is required for login.");
        }

        var user = await _userManager.Users
            .Include(u => u.UserInfo)
            .Include(u => u.Cart)
            .FirstOrDefaultAsync(u => u.Email == loginModelView.Email
                                      || u.PhoneNumber == loginModelView.PhoneNumber
                                      || u.UserName == loginModelView.UserName);

        if (user is null)
        {
            throw new BaseException.UnauthorizedException("unauthorized", "Incorrect user login credentials");
        }

        if (user.Status != Constants.UserActiveStatus) 
        {
            throw new BaseException.UnauthorizedException("unauthorized", "This account has been disabled.");
        }

        var success = await _userManager.CheckPasswordAsync(user, loginModelView.Password);

        if (success)
        {
            var userResponse = await CreateUserResponse(user);
            return userResponse;
        }

        throw new BaseException.UnauthorizedException("incorrect_password", "Incorrect password");
        // return (false, "Incorrect password", null, StatusCodes.Status401Unauthorized);
    }

    public async Task<UserLoginResponseModel> CreateUserResponse(ApplicationUser user)
    {
        var token = await _tokenService.CreateToken(user); 
        return new UserLoginResponseModel()
        {
            FullName = user.UserInfo.FullName,
            UserName = user.UserName,
            DisplayName = user.UserInfo.DisplayName,
            Token = token
        };
    }
    public async Task<bool> AssignRoleToUser(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to add role to user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        return true;
    }
}