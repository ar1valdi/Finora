using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Finora.Repositories.Interfaces;

namespace Finora.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        var repositoryInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && t.Name.StartsWith("I") && t.Name.EndsWith("Repository"))
            .ToList();

        var repositoryImplementations = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"))
            .ToList();

        foreach (var repositoryInterface in repositoryInterfaces)
        {
            var implementation = repositoryImplementations
                .FirstOrDefault(impl => repositoryInterface.IsAssignableFrom(impl));

            if (implementation != null)
            {
                services.AddScoped(repositoryInterface, implementation);
            }
        }

        services.AddScoped<IUnitOfWork, Repositories.UnitOfWork>();

        return services;
    }
}
