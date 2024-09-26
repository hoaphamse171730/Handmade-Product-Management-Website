using HandmadeProductManagement.Contract.Services.Interface;

namespace HandmadeProductManagementAPI.BackgroundServices
{
    public class PaymentExpirationBackgroundService : BackgroundService
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentExpirationBackgroundService> _logger;

        public PaymentExpirationBackgroundService(IPaymentService paymentService, ILogger<PaymentExpirationBackgroundService> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _paymentService.CheckAndExpirePaymentsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking and expiring payments.");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); 
            }
        }
    }
}