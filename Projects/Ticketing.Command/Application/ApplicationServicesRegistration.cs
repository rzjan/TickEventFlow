using FluentValidation;
using Ticketing.Command.Application.Core;
using Ticketing.Command.Application.Models;



namespace Ticketing.Command.Application;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {
        services.Configure<MongoSettings>(configuration.GetSection("MongoSettings"));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationServicesRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof(ApplicationServicesRegistration).Assembly);

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        return services;
    }
}
