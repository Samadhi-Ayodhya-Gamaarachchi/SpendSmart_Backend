using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using SpendSmart_Backend.Services;
using Microsoft.Extensions.Logging;

public class RecurringTransactionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringTransactionBackgroundService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Run every 5 minutes instead of 1 hour

    public RecurringTransactionBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RecurringTransactionBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Transaction Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Processing recurring transactions at {Time}", DateTime.UtcNow);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var recurringTransactionService = scope.ServiceProvider
                        .GetRequiredService<IRecurringTransactionService>();
                    
                    await recurringTransactionService.ProcessRecurringTransactionsAsync();
                }

                _logger.LogInformation("Recurring transactions processed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing recurring transactions at {Time}", DateTime.UtcNow);
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Recurring Transaction Background Service stopped.");
    }
}