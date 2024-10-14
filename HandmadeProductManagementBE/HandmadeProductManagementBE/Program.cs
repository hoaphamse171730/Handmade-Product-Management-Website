using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Services.SignalRHub;
using HandmadeProductManagementAPI.Extensions;
using HandmadeProductManagementAPI.Middlewares;
using HandmadeProductManagementBE.API;
using Microsoft.AspNetCore.Identity;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.SignalR;
using Serilog;



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers();
//All extra services must be contained in ApplicationServiceExtensions & IdentityServiceExtensions
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddConfig(builder.Configuration);
builder.Services.RegisterMapsterConfiguration();
builder.Services.ConfigureFluentValidation();
builder.Services.AddFireBaseServices();
builder.Services.ConfigureGraphql();
builder.Services.ConfigureSwaggerServices();


// CORS for React application
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("https://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRoles(services); // Call the method to seed roles
}

/// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
});
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");


app.UseAuthentication(); 
app.UseRouting();       
app.UseAuthorization();  

app.MapGraphQL();
app.UseMiddleware<RequestLoggingMiddleware>();
app.MapControllers();
app.MapPost("broadcast", async (string message, IHubContext<ChatHub, IChatClient> context) => {
    await context.Clients.All.ReceiveMessage(message);
    return Results.NoContent();
});
app.Run();


static async Task SeedRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    string[] roleNames = { "Admin", "Seller", "Customer" };

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Create new ApplicationRole instead of IdentityRole
            var role = new ApplicationRole { Name = roleName };
            await roleManager.CreateAsync(role);
        }
    }
}
