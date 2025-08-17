// See https://aka.ms/new-console-template for more information
using Finora.Models;
using Finora.Persistance.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Finora.MocksGenerator;

Console.WriteLine("Hello, World!");


var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var connectionString = "Host=127.0.0.1;Port=5432;Database=finora;Username=admin;Password=admin";
                services.AddDbContext<FinoraDbContext>(options =>
                options.UseNpgsql(connectionString));
            })
            .Build();

using var scope = host.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<FinoraDbContext>();

db.Database.Migrate();

var users = Mocks.GenerateMockUsers();
db.Users.AddRange(users);
db.SaveChanges();

foreach (var user in db.Users.ToList())
{
    Console.WriteLine($"User: {user.FullName}");
}