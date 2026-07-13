namespace Ticketing.Command.Features.Apis;

public static class EndpointRouteBuilderExtensions
{
    // Extiende IEndpointRouteBuilder para mapear automáticamente todos
    // los endpoints definidos por las implementaciones de IMinimalApi
    public static IEndpointRouteBuilder MapMinimalApiEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        // Obtiene una instancia de IMinimalApi desde el contenedor de servicios
        var minimalApis = endpointRouteBuilder.ServiceProvider.GetServices<IMinimalApi>();

        // Llama al método AddEnpoint para cada instancia de IMinimalApi
        foreach (IMinimalApi minimalApi in minimalApis)
        {         
            minimalApi.AddEnpoint(endpointRouteBuilder);
        }

        return endpointRouteBuilder;
    }
}
