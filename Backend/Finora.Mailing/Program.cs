using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Finora.Mailing.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<RabbitMqListener>();
    })
    .Build();

Console.WriteLine("Starting Finora Mailing Service...");

var cts = new CancellationTokenSource();
var cancellationToken = cts.Token;

var task = host.RunAsync(cancellationToken);

Console.WriteLine("Enter 'quit' to stop the service");

var consoleInput = Console.ReadLine();
while (consoleInput != "quit")
{
    consoleInput = Console.ReadLine();
}

cts.Cancel();
await task;