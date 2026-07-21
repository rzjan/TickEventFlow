using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Ticketing.Query.Infrastructure.Persistence;

namespace Ticketing.Query.Application.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigration(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var service = scope.ServiceProvider;
            var loggerFactory = service.GetRequiredService<ILoggerFactory>();

            try
            {
                var contextFacotry = service.GetRequiredService<DataBaseContextFactory>();                
                using TicketDbContext dbContext = contextFacotry.CreateDbContext();
                await dbContext.Database.MigrateAsync();

            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred while migrating the database.");                
            }
        }
    }
}
