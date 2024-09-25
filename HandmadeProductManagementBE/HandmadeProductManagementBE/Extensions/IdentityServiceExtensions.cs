using HandmadeProductManagement.Repositories.Context;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.AspNetCore.Identity;

namespace HandmadeProductManagementAPI.Extensions;

public static class IdentityServiceExtensions
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

            //----- Wait til Identity added
        
        // var key = new SymmetricSecurityKey(
        //     Encoding.UTF8.GetBytes(config["TokenKey"]));
        //
        // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //     .AddJwtBearer(opt =>
        //     {
        //         opt.TokenValidationParameters = new TokenValidationParameters()
        //         {
        //             ValidateIssuerSigningKey = true,
        //             IssuerSigningKey = key,
        //             ValidateIssuer = false,
        //             ValidateAudience = false
        //         };
        //         opt.Events = new JwtBearerEvents()
        //         {
        //             OnMessageReceived = context =>
        //             {
        //                 //fixed token key
        //                 var accessToken = context.Request.Query["access_token"];
        //                 var path = context.HttpContext.Request.Path;
        //                 if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chat")))
        //                 {
        //                     context.Token = accessToken;
        //                 }
        //
        //                 return Task.CompletedTask;
        //             }
        //         };
        //     });
        // services.AddAuthorization(opt =>
        // {
        //     opt.AddPolicy("IsActivityHost", policy => { policy.Requirements.Add(new IsHostRequirement()); });
        // });
        // services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
        // services.AddScoped<TokenService>();

        return services;
    }
}