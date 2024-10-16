using System.Text;
using HandmadeProductManagement.Repositories.Context;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HandmadeProductManagement.Services.Service;

public static class AddIdentityService
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddIdentityCore<ApplicationUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                opt.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<DatabaseContext>();

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["TokenKey"]));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                opt.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        //fixed token key
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrWhiteSpace(accessToken) && (path.StartsWithSegments("/chat")))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();
        // services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
        services.AddScoped<TokenService>();

        return services;
    }
}