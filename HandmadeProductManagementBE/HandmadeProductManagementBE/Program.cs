using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagementAPI.Extensions;
using HandmadeProductManagementBE.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// config appsettings by env
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

//All extra services must be contained in ApplicationServiceExtentions & IdentityServiceExtensions
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddConfig(builder.Configuration);
builder.Services.RegisterMapsterConfiguration();
builder.Services.ConfigureFluentValidation();



var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
            if (exception is null) return;

            var problemDetails = new ProblemDetails()
            {
                Title = exception.Message,
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.StackTrace?.TrimStart()
            };

            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exception, exception.Message);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    }
);

app.Run();
