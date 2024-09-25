using HandmadeProductManagement.Repositories.Context;
using HandmadeProductManagementBE.API;
using Microsoft.EntityFrameworkCore;
using HandmadeProductManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// config appsettings by env
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BloggingDatabase"),
    sqlOptions => sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5, // Number of retry attempts
        maxRetryDelay: TimeSpan.FromSeconds(10), // Delay between retries
        errorNumbersToAdd: null // Additional error codes to consider as transient errors
    ).MigrationsAssembly("HandmadeProductManagementAPI")));
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BloggingDatabase"))
           .EnableSensitiveDataLogging() // Enable detailed logging
           .EnableDetailedErrors()); // Enable detailed error messages

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddInfrastructure(builder.Configuration);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddConfig(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
