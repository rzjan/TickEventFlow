using System.Reflection;

namespace Ticketing.Command.Features.Apis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRegisterMinimalApis(this IServiceCollection services) 
    {
        // Register all implementations of IMinimalApi in the current assembly
        var currentAssembly = Assembly.GetExecutingAssembly();        
        var minimalApis = currentAssembly.GetTypes()
             .Where(t => typeof(IMinimalApi).IsAssignableFrom(t) && t != typeof(IMinimalApi) && t.IsPublic && !t.IsAbstract);

        // Register each implementation as a singleton service
        foreach (var minialApi in minimalApis)
        {
            services.AddSingleton(typeof(IMinimalApi), minialApi);
        }
        return services;
    }
}
