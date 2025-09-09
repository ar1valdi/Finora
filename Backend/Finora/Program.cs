using Finora.Persistance.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Finora.Web.Configuration;
using Finora.Web.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Finora.Handlers;
using Finora.Services;
using Mapster;
using Finora.Backend.Persistance.Configs.Adapters;
using Finora.Backend.Common.Extensions;
using MediatR;
using Finora.Validation;

var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<FinoraDbContext>(options =>
                    options.UseNpgsql(connectionString, npgsqlOptions => 
                    {
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "Finora");
                    })
                );

                services.Configure<RabbitMqConfiguration>(
                    context.Configuration.GetSection("RabbitMQ"));

                services.Configure<MailingConfiguration>(
                    context.Configuration.GetSection("Mailing"));

                services.AddSingleton<IRabbitMqService, RabbitMqService>();
                services.AddScoped<IPasswordService, PasswordService>();
                services.AddTransient<IRabbitListener, RabbitListener>();
                services.AddTransient<IOutboxSender, OutboxSender>();
                services.AddScoped<IMailingService, MailingService>();
                services.AddScoped<IDbHealthCheck, DbHealthCheck>();
                services.AddScoped<IRequestValidator, RequestValidator>();
                
                services.AddRepositories();
                
                TypeAdapterConfig.GlobalSettings.Scan(typeof(UserTypeAdapter).Assembly);
                
                services.AddMediatR(cfg => {
                    cfg.RegisterServicesFromAssembly(typeof(GetAllUsersHandler).Assembly);
                    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                });
            })
            .Build();

using var startupScope = host.Services.CreateScope();
using var commandsScope = startupScope.ServiceProvider.CreateScope();
using var queriesScope = startupScope.ServiceProvider.CreateScope();
using var outboxSenderScope = startupScope.ServiceProvider.CreateScope();

var db = startupScope.ServiceProvider.GetRequiredService<FinoraDbContext>();
var commandsListener = commandsScope.ServiceProvider.GetRequiredService<IRabbitListener>();
var queriesListener = queriesScope.ServiceProvider.GetRequiredService<IRabbitListener>();
var outboxSender = outboxSenderScope.ServiceProvider.GetRequiredService<IOutboxSender>();
var rabbitConfig = startupScope.ServiceProvider.GetRequiredService<IOptions<RabbitMqConfiguration>>();
var rabbitMqService = startupScope.ServiceProvider.GetRequiredService<IRabbitMqService>();

db.Database.Migrate();

await rabbitMqService.EnsureTopology(CancellationToken.None);

var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var listeners = new List<Task> {
    commandsListener.Listen(
        rabbitConfig.Value.RequestQueue,
        "Command",
        cancellationToken),

    queriesListener.Listen(
        rabbitConfig.Value.QueryQueue,
        "Query",
        cancellationToken),

    outboxSender.Run(100, cancellationToken)
};

var consoleInput = Console.ReadLine();
while (consoleInput != "quit")
{
    consoleInput = Console.ReadLine();
}

cancellationTokenSource.Cancel();

await Task.WhenAll(listeners);
