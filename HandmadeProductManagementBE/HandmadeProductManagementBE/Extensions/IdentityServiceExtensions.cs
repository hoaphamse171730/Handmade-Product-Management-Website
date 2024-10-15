using System.Security.Claims;
using System.Text;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Repositories.Context;
using HandmadeProductManagement.Repositories.Entity;
using HandmadeProductManagement.Services.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace HandmadeProductManagementAPI.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
            opt.User.RequireUniqueEmail = false;
        })
        .AddDefaultTokenProviders() 
        .AddEntityFrameworkStores<DatabaseContext>();  

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["TokenKey"]!));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role,  // Ensure this matches your token's role claim
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        //services.AddAuthorization();
        services.AddAuthorization();
        services.AddScoped<TokenService>();
        return services;
    }
}