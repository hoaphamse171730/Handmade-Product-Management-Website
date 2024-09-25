using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.UserModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
//using Castle.Core.Configuration;
namespace HandmadeProductManagement.Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration; // For accessing JWT settings
        public UserService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public Task<IList<UserResponseModel>> GetAll()
        {
            IList<UserResponseModel> users = new List<UserResponseModel>
            {
                new UserResponseModel { Id = "1" },
                new UserResponseModel { Id = "2" },
                new UserResponseModel { Id = "3" }
            };

            return Task.FromResult(users);
        }

        public async Task<UserResponseModel> GetUserByCredentials(string username, string password)
        {
            IQueryable<ApplocationUserLogins> query = _unitOfWork.GetRepository<ApplocationUserLogins>().Entities;

            // Retrieve the user by username
            var user = await query.FirstOrDefaultAsync(u => u.UserName == username);

            // If the user is found and the password matches, return the UserResponseModel
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return new UserResponseModel
                {
                    Id = user.Id.ToString(),
                    UserName = user.UserName,
                    UserEmail = user.Email

                };
            }
      
            return null;
        }

        public async Task<string> LoginUser(string username, string password)
        {
            // Validate user credentials
            var user = await GetUserByCredentials(username, password);
            if (user == null)
            {
                return null; // Invalid credentials
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);
            return token;
        }

        private string GenerateJwtToken(UserResponseModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserEmail),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(120), 
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
