using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Ticketing.Query.Domain.Abstractions;
using Ticketing.Query.Infrastructure.Persistence;
using Ticketing.Query.Infrastructure.Repositories;

namespace Ticketing.Query.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection RegisterInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        //services.AddDbContext<TicketDbContext>(options =>
        //options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        Action<DbContextOptionsBuilder> configureDbContext;
        var connectionString = configuration
                                    .GetConnectionString("PostgresConnectionString")
                                    ?? throw new ArgumentException(nameof(configuration));

        configureDbContext = options => options.UseLazyLoadingProxies().UseNpgsql(connectionString);

        // Register the DbContext with the configured options
        services.AddDbContext<TicketDbContext>(opt=> opt.UseNpgsql(connectionString));
        services.AddSingleton<DataBaseContextFactory>(new DataBaseContextFactory(configureDbContext));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.Configure<ConsumerConfig>(configuration.GetSection(nameof(ConsumerConfig)));
        return services;
    }
}