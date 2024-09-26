using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Repositories.UOW;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Services.Service;

namespace HandmadeProductManagement.Services
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRepositories();
            // services.AddServices();
        }
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        // public static void AddServices(this IServiceCollection services)
        // {
        //     services.AddScoped<IProductService, ProductService>();
        // }

    }
}
