using HomeLabCore.Application.Interfaces;
using HomeLabCore.Infrastructure.Database;
using HomeLabCore.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeLabCore.Infrastructure.Initializers;

internal sealed class DatabaseInitializer(
    ApplicationDbContext dbContext, 
    ILogger<DatabaseInitializer> logger) 
    : IApplicationInitializer
{
    public int Order => -100;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.MigratingDatabase();

            await dbContext.Database.MigrateAsync(cancellationToken);

            logger.MigratedDatabaseSuccessfully();
        }
        catch (Exception ex)
        {
            logger.DatabaseMigrationFailed(ex);

            throw;
        }
    }
}
