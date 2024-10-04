using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagementAPI.BackgroundServices
{
    public class PromotionExpirationBackgroundService : BackgroundService
    {
        private readonly ILogger<PromotionExpirationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PromotionExpirationBackgroundService(ILogger<PromotionExpirationBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Promotion Expiration Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Checking for expired promotions...");
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                await CheckAndExpirePromotionsAsync(stoppingToken);
            }
            _logger.LogInformation("Promotion Expiration Background Service is stopping.");
        }

        private async Task CheckAndExpirePromotionsAsync(CancellationToken stoppingToken)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var promotionRepository = unitOfWork.GetRepository<Promotion>();
                    var expiredPromotions = await promotionRepository.Entities
                        .Where(p => p.EndDate < DateTime.UtcNow && p.Status != "Inactive")
                        .ToListAsync(stoppingToken);

                    if (expiredPromotions.Any())
                    {
                        foreach (var promotion in expiredPromotions)
                        {
                            promotion.Status = "Inactive";
                            promotion.LastUpdatedTime = DateTime.UtcNow;
                            promotion.LastUpdatedBy = "System"; 
                        }
                        await unitOfWork.SaveAsync();
                        _logger.LogInformation($"{expiredPromotions.Count} promotions have been marked as expired.");
                    }
                    else
                    {
                        _logger.LogInformation("No expired promotions found.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while expiring promotions.");
            }
        }
    }
}
