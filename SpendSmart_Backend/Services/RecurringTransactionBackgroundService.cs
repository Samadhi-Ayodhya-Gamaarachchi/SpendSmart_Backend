using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using SpendSmart_Backend.Services;

public class RecurringTransactionBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Adjust interval as needed

    public RecurringTransactionBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var recurringTransactionService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();
                await recurringTransactionService.ProcessRecurringTransactionsAsync();
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }
}