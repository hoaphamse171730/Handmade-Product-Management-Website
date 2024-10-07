using HandmadeProductManagementAPI.Extensions;
using HandmadeProductManagementAPI.Middlewares;
using HandmadeProductManagementBE.API;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
//
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers(
//    opt =>
//{
//    var policy = new AuthorizationPolicyBuilder().Build();
//    opt.Filters.Add(new AuthorizeFilter(policy));
//}
);

//All extra services must be contained in ApplicationServiceExtensions & IdentityServiceExtensions
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddConfig(builder.Configuration);
builder.Services.RegisterMapsterConfiguration();
builder.Services.ConfigureFluentValidation();
builder.Services.AddFireBaseServices();


var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<RequestLoggingMiddleware>();
//configure the app to use Custom Exception Handler globally
app.UseExceptionHandler(options => { });

app.Run();