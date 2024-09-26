using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Repositories.Context;
using HandmadeProductManagement.Repositories.UOW;
using HandmadeProductManagement.Services.Service;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.Extensions;

public static class ApplicationServiceExtenstions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(config.GetConnectionString("BloggingDatabase"),
                b => b.MigrationsAssembly("HandmadeProductManagementAPI")));

        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy",
                policy => { policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithOrigins("https://localhost:7159"); });
        });

        services.AddScoped<ICancelReasonService, CancelReasonService>();
        services.AddScoped<IStatusChangeService, StatusChangeService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IShopService, ShopService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();


        return services;
    }
}