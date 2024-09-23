using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Repositories.UOW;

namespace HandmadeProductManagement.Services
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRepositories();
        }
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
