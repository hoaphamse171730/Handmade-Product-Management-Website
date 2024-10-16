using System.IdentityModel.Tokens.Jwt;
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
using System.Web;

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
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageMissingLoginIdentifier);
        }

        if (!string.IsNullOrWhiteSpace(loginModelView.Email) && !IsValidEmail(loginModelView.Email))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidEmailFormat);
        }

        if (!string.IsNullOrWhiteSpace(loginModelView.UserName) && !IsValidUsername(loginModelView.UserName))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidUsernameFormat);
        }

        if (!string.IsNullOrWhiteSpace(loginModelView.PhoneNumber) && !IsValidPhoneNumber(loginModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidPhoneFormat);
        }

        var user = await _userManager.Users
            .Include(u => u.UserInfo)
            .FirstOrDefaultAsync(u => u.Email == loginModelView.Email
                                      || u.PhoneNumber == loginModelView.PhoneNumber
                                      || u.UserName == loginModelView.UserName) ?? throw new BaseException.UnauthorizedException(StatusCodeHelper.Unauthorized.ToString(),

                Constants.ErrorMessageUnauthorized);
        if (user.Status != Constants.UserActiveStatus)
        {
            throw new BaseException.UnauthorizedException(StatusCodeHelper.Unauthorized.ToString(),
                Constants.ErrorMessageAccountDisabled);
        }

        var success = await _userManager.CheckPasswordAsync(user, loginModelView.Password);

        if (!success)
        {
            throw new BaseException.UnauthorizedException(StatusCodeHelper.Unauthorized.ToString(),
                Constants.ErrorMessageIncorrectPassword);
        }

        var userResponse = await CreateUserResponse(user);
        return BaseResponse<UserLoginResponseModel>.OkResponse(userResponse);
    }

    public async Task<BaseResponse<string>> RegisterAsync(RegisterModelView registerModelView)
    {
        ValidateRegisterModel(registerModelView);

        if (await _userManager.Users.AnyAsync(u => u.UserName == registerModelView.UserName))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageUsernameTaken);
        }

        if (await _userManager.Users.AnyAsync(u => u.Email == registerModelView.Email))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageEmailTaken);
        }

        if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == registerModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessagePhoneTaken);
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

                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageUserCreationFailed + string.Join("; ", errorMessages));
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
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageUsernameTaken);
        }

        if (await _userManager.Users.AnyAsync(u => u.Email == registerModelView.Email))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageEmailTaken);
        }

        if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == registerModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessagePhoneTaken);
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

                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    Constants.ErrorMessageUserCreationFailed + string.Join("; ", errorMessages));
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
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidEmailFormat);
        }

        if (!IsValidUsername(registerModelView.UserName))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidUsernameFormat);
        }

        if (!ValidationHelper.IsValidNames(CustomRegex.FullNameRegex, registerModelView.FullName))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidFullnameFormat);
        }

        if (!IsValidPhoneNumber(registerModelView.PhoneNumber))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidPhoneFormat);
        }

        if (string.IsNullOrWhiteSpace(registerModelView.Password) || !IsValidPassword(registerModelView.Password))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageWeakPassword);
        }
    }

    public async Task<BaseResponse<string>> ForgotPasswordAsync(ForgotPasswordModelView forgotPasswordModelView)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordModelView.Email);
        if (user == null || !user.EmailConfirmed)
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                Constants.ErrorMessageInvalidEmail);
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = HttpUtility.UrlEncode(resetToken);
        var passwordResetLink = $"{forgotPasswordModelView.ClientUri}?email={user.Email}&token={encodedToken}";

        await _emailService.SendPasswordRecoveryEmailAsync(user.Email!, passwordResetLink);

        return new BaseResponse<string>()
        {
            StatusCode = StatusCodeHelper.OK,
            Message = Constants.MessagePasswordResetLinkSent
        };
    }

    public async Task<BaseResponse<string>> ResetPasswordAsync(ResetPasswordModelView resetPasswordModelView)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordModelView.Email)
                    ?? throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidEmail);

        var decodedToken = HttpUtility.UrlDecode(resetPasswordModelView.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordModelView.NewPassword);

        if (result.Succeeded)
        {
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.OK,
                Message = Constants.MessagePasswordResetSuccess
            };
        }

        throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageResetPasswordError);
    }

    public async Task<BaseResponse<string>> ConfirmEmailAsync(ConfirmEmailModelView confirmEmailModelView)
    {
        if (string.IsNullOrWhiteSpace(confirmEmailModelView.Email) || !IsValidEmail(confirmEmailModelView.Email))
        {
            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidEmail);
        }

        var user = await _userManager.FindByEmailAsync(confirmEmailModelView.Email)
                    ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

        var decodedToken = HttpUtility.UrlDecode(confirmEmailModelView.Token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            return new BaseResponse<string>()
            {
                StatusCode = StatusCodeHelper.OK,
                Message = Constants.MessageEmailConfirmedSuccess
            };
        }
        else
        {
            var errorMessages = result.Errors
                .Select(e => e.Description)
                .ToList();

            throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                $"{Constants.ErrorMessageEmailConfirmationError}: {string.Join("; ", errorMessages)}");
        }
    }

    public async Task<BaseResponse<string>> GoogleLoginAsync(string token)
    {
        return await ProcessLoginAsync(token);
    }

    public async Task<BaseResponse<string>> FacebookLoginAsync(string token)
    {
        return await ProcessLoginAsync(token);
    }

    private async Task<BaseResponse<string>> ProcessLoginAsync(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        if (jsonToken == null)
        {
            return BaseResponse<string>.FailResponse(Constants.ErrorMessageInvalidToken);
        }

        var email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;
        var name = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value;
        var picture = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "picture")?.Value;

        if (email == null || name == null)
        {
            return BaseResponse<string>.FailResponse(Constants.ErrorMessageMissingClaims);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BaseResponse<string>.FailResponse(Constants.ErrorMessageUserCreationFailed);
            }

            await _userManager.AddToRoleAsync(user, "Customer");
        }

        var userToken = await _tokenService.CreateToken(user);
        return BaseResponse<string>.OkResponse(userToken);
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
        var user = await _userManager.FindByIdAsync(userId) ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(),
                    $"{Constants.ErrorMessageRoleAssignmentFailed}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
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