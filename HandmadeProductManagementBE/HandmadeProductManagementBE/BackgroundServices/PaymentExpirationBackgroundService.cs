using HandmadeProductManagement.Contract.Services.Interface;

namespace HandmadeProductManagementAPI.BackgroundServices
{
    public class PaymentExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<PaymentExpirationBackgroundService> _logger;

        public PaymentExpirationBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<PaymentExpirationBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
                    try
                    {
                        await paymentService.CheckAndExpirePaymentsAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred while checking and expiring payments.");
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}