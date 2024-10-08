using FluentValidation;
using FluentValidation.AspNetCore;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Exceptions.Handler;
using HandmadeProductManagement.ModelViews.AuthModelViews;
using HandmadeProductManagement.ModelViews.OrderDetailModelViews;
using HandmadeProductManagement.ModelViews.ProductModelViews;
using HandmadeProductManagement.ModelViews.PromotionModelViews;
using HandmadeProductManagement.Repositories.Context;
using HandmadeProductManagement.Repositories.Entity;
using HandmadeProductManagement.Repositories.UOW;
using HandmadeProductManagement.Services.Service;
using HandmadeProductManagement.Validation.OrderDetail;
using HandmadeProductManagement.Validation.Authentication;
using HandmadeProductManagement.Validation.Product;
using HandmadeProductManagement.Validation.Promotion;
using HandmadeProductManagementAPI.BackgroundServices;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHttpContextAccessor();
        services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(config.GetConnectionString("BloggingDatabase"),
                b => b.MigrationsAssembly("HandmadeProductManagementAPI")));

        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy",
                policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithOrigins("https://localhost:7159");
                });
        });
        
        services.AddExceptionHandler<CustomExceptionHandler>(); //NA
        
        services.AddFluentValidationAutoValidation();
        
        services.AddValidatorsFromAssemblyContaining<RegisterModelViewValidator>();
        
        services.AddScoped<ICancelReasonService, CancelReasonService>();
        services.AddScoped<IStatusChangeService, StatusChangeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IOrderDetailService, OrderDetailService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IShopService, ShopService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IReplyService, ReplyService>();
        services.AddScoped<IPaymentDetailService, PaymentDetailService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddHostedService<PaymentExpirationBackgroundService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICartItemService, CartItemService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserAgentService, UserAgentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<AuthenticationService>();
        return services;
    }

    public static void ConfigureFluentValidation(this IServiceCollection services)
    {
        #region Promotion
        services.AddScoped<IValidator<PromotionForCreationDto>, PromotionForCreationDtoValidator>();
        services.AddScoped<IValidator<PromotionForUpdateDto>, PromotionForUpdateDtoValidator>();
        #endregion

        #region Product
        services.AddScoped<IValidator<ProductForCreationDto>, ProductForCreationDtoValidator>();
        services.AddScoped<IValidator<ProductForUpdateDto>, ProductForUpdateDtoValidator>();
        #endregion

        #region OrderDetail
        services.AddScoped<IValidator<OrderDetailForCreationDto>, OrderDetailForCreationDtoValidator>();
        services.AddScoped<IValidator<OrderDetailForUpdateDto>, OrderDetailForUpdateDtoValidator>();
        #endregion



        services.AddScoped<IValidator<RegisterModelView>, RegisterModelViewValidator>();
    }

    public static void RegisterMapsterConfiguration(this IServiceCollection services)
    {
        TypeAdapterConfig<RegisterModelView, ApplicationUser>
            .NewConfig()
            .Map(dest => dest.UserInfo.FullName, src => src.FullName)
            .Map(dest => dest.CartId, () => Guid.NewGuid())
            .Map(dest => dest.CreatedBy, src => src.UserName)
            .Map(dest => dest.LastUpdatedBy, src => src.UserName)
            
            .Map(dest => dest.Cart.CreatedBy, src => src.UserName)
            .Map(dest => dest.Cart.LastUpdatedBy, src => src.UserName)
            ;
    }
}