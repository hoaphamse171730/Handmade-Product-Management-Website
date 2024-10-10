using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.Core.Utils;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly IEmailService _emailService;

    public AuthenticationService(UserManager<ApplicationUser> userManager, TokenService tokenService, IEmailService emailService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    public async Task<BaseResponse<UserLoginResponseModel>> LoginAsync(LoginModelView loginModelView)
    {
        if (string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) &&
            string.IsNullOrWhiteSpace(loginModelView.Email) &&
            string.IsNullOrWhiteSpace(loginModelView.UserName) ||
            string.IsNullOrWhiteSpace(loginModelView.Password))
        {
            throw new BaseException.BadRequestException("missing_login_identifier",
                "At least one of Phone Number, Email, or Username is required for login.");
        }

        if (!string.IsNullOrWhiteSpace(loginModelView.Email) && !IsValidEmail(loginModelView.Email))
        {
            throw new BaseException.BadRequestException("invalid_email_format",
                "Invalid Email format.");
        }

        if (!string.IsNullOrWhiteSpace(loginModelView.UserName) && !IsValidUsername(loginModelView.UserName))
        {
            throw new BaseException.BadRequestException("invalid_username_format",
                "Invalid Username format. Special characters are not allowed.");
        }

        if (!string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) && !IsValidPhoneNumber(loginModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException("invalid_phone_format",
                "Invalid Phone Number format. Phone number must be between 10 to 11 digits and start with 0.");
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

        if (!success)
        {
            throw new BaseException.UnauthorizedException("incorrect_password", "Incorrect password");
        }

        var userResponse = await CreateUserResponse(user);
        return BaseResponse<UserLoginResponseModel>.OkResponse(userResponse);
    }

    public async Task<BaseResponse<string>> RegisterAsync(RegisterModelView registerModelView)
    {
        ValidateRegisterModel(registerModelView);

        if (await _userManager.Users.AnyAsync(u => u.UserName == registerModelView.UserName))
        {
            throw new BaseException.BadRequestException("username_taken", "Username is already taken.");
        }

        if (await _userManager.Users.AnyAsync(u => u.Email == registerModelView.Email))
        {
            throw new BaseException.BadRequestException("email_taken", "Email is already taken.");
        }

        if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == registerModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException("phone_taken", "Phone number is already taken.");
        }


        var user = registerModelView.Adapt<ApplicationUser>();

        try
        {
            var result = await _userManager.CreateAsync(user, registerModelView.Password);

            if (!result.Succeeded)
            {
                var errorMessages = result.Errors
                    .Select(e => e.Description)
                    .ToList();

                throw new BaseException.BadRequestException("user_creation_failed",
                    "User creation failed: " + string.Join("; ", errorMessages));
            }

            await _emailService.SendEmailConfirmationAsync(user.Email!, registerModelView.ClientUri);
            await AssignRoleToUser(user.Id.ToString(), "Seller");

            return BaseResponse<string>.OkResponse(user.Id.ToString());
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    public async Task<BaseResponse<string>> RegisterAdminAsync(RegisterModelView registerModelView)
    {
        ValidateRegisterModel(registerModelView);

        if (await _userManager.Users.AnyAsync(u => u.UserName == registerModelView.UserName))
        {
            throw new BaseException.BadRequestException("username_taken", "Username is already taken.");
        }

        if (await _userManager.Users.AnyAsync(u => u.Email == registerModelView.Email))
        {
            throw new BaseException.BadRequestException("email_taken", "Email is already taken.");
        }

        if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == registerModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException("phone_taken", "Phone number is already taken.");
        }


        var user = registerModelView.Adapt<ApplicationUser>();

        try
        {
            var result = await _userManager.CreateAsync(user, registerModelView.Password);

            if (!result.Succeeded)
            {
                var errorMessages = result.Errors
                    .Select(e => e.Description)
                    .ToList();

                throw new BaseException.BadRequestException("user_creation_failed",
                    "User creation failed: " + string.Join("; ", errorMessages));
            }

            await _emailService.SendEmailConfirmationAsync(user.Email!, registerModelView.ClientUri);
            await AssignRoleToUser(user.Id.ToString(), "Admin");

            return BaseResponse<string>.OkResponse(user.Id.ToString());
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    private void ValidateRegisterModel(RegisterModelView registerModelView)
    {
        if (!IsValidEmail(registerModelView.Email))
        {
            throw new BaseException.BadRequestException("invalid_email_format",
                "Invalid Email format.");
        }

        if (!IsValidUsername(registerModelView.UserName))
        {
            throw new BaseException.BadRequestException("invalid_username_format",
                "Invalid Username format. Special characters are not allowed.");
        }

        if (!ValidationHelper.IsValidNames(CustomRegex.FullNameRegex, registerModelView.FullName))
        {
            throw new BaseException.BadRequestException("invalid_fullname_format",
                "Full Name contains invalid characters.");
        }

        if (!IsValidPhoneNumber(registerModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException("invalid_phone_format",
                "Invalid Phone Number format. Phone number must be between 10 to 11 digits and start with 0.");
        }

        if (string.IsNullOrWhiteSpace(registerModelView.Password) || !IsValidPassword(registerModelView.Password))
        {
            throw new BaseException.BadRequestException("weak_password",
                "Password is too weak. It must be at least 8 characters long, contain uppercase, lowercase, a special character, and a digit.");
        }
    }


    private async Task<UserLoginResponseModel> CreateUserResponse(ApplicationUser user)
    {
        var token = await _tokenService.CreateToken(user);
        return new UserLoginResponseModel
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

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
   
    private bool IsValidUsername(string username)
    {
        return !System.Text.RegularExpressions.Regex.IsMatch(username, "[^a-zA-Z0-9]");
    }
    
    private bool IsValidPhoneNumber(string phoneNumber)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, "^0[0-9]{9,10}$");
    }
    private bool IsValidPassword(string password)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+={}[\]|\\:;'\<>,.?/~`])[A-Za-z\d!@#$%^&*()_+={}[\]|\\:;'\<>,.?/~`]{8,}$");
    }
}