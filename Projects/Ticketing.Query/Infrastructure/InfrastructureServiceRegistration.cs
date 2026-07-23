using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Domain.Employees;
using Ticketing.Query.Infrastructure.Consumers;
using Ticketing.Query.Infrastructure.Persistence;
using Ticketing.Query.Infrastructure.Persistence.Interceptors;
using Ticketing.Query.Infrastructure.Repositories;

namespace Ticketing.Query.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection RegisterInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {        
        Action<DbContextOptionsBuilder> configureDbContext;
        services.AddSingleton<AuditEntitiesInterceptor>();
        var connectionString = configuration
                                    .GetConnectionString("PostgresConnectionString")
                                    ?? throw new ArgumentException(nameof(configuration));

        configureDbContext = options => options.UseLazyLoadingProxies()
                                               .UseNpgsql(connectionString)
                                               .AddInterceptors( new AuditEntitiesInterceptor());


        // Register the DbContext with the configured options
        services.AddDbContext<TicketDbContext>(opt => opt.UseNpgsql(connectionString));
        services.AddSingleton<DataBaseContextFactory>(new DataBaseContextFactory(configureDbContext));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.Configure<ConsumerConfig>(configuration.GetSection(nameof(ConsumerConfig)));

        //regisro la clase ConsumerHostedService que es el proceso en backround
        services.AddHostedService<ConsumerHostedService>();
        services.AddScoped<IEventHandler, Handlers.EventHanlder>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        return services;
    }
}