using Finora.Persistance.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Finora.Web.Configuration;
using Finora.Web.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<FinoraDbContext>(options =>
                    options.UseNpgsql(connectionString, npgsqlOptions => 
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "Finora"))
                );

                services.Configure<RabbitMqConfiguration>(
                    context.Configuration.GetSection("RabbitMQ"));

                services.AddSingleton<IRabbitMqService, RabbitMqService>();
                services.AddTransient<IRabbitListener, RabbitListener>();
            })
            .Build();

using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<FinoraDbContext>();
var commandsListener = scope.ServiceProvider.GetRequiredService<IRabbitListener>();
var queriesListener = scope.ServiceProvider.GetRequiredService<IRabbitListener>();
var rabbitConfig = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqConfiguration>>();
var rabbitMqService = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();

db.Database.Migrate();

await rabbitMqService.EnsureTopology(CancellationToken.None);

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var listeners = new List<Task> {
    commandsListener.Listen(
        rabbitConfig.Value.RequestQueue,
        cancellationToken),

    queriesListener.Listen(
        rabbitConfig.Value.QueryQueue,
        cancellationToken),
};

var consoleInput = Console.ReadLine();
while (consoleInput != "quit")
{
    consoleInput = Console.ReadLine();
}

cancellationTokenSource.Cancel();

await Task.WhenAll(listeners);
