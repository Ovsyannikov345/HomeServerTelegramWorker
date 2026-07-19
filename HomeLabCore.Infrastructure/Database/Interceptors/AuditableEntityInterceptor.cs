using HomeLabCore.Domain.Entities;
using HomeLabCore.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HomeLabCore.Infrastructure.Database.Interceptors;

public sealed class AuditableEntityInterceptor(ILogger<AuditableEntityInterceptor> logger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFieldsAndLog(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFieldsAndLog(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFieldsAndLog(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<EntityBase>();

        foreach (var entry in entries)
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.CreatingNewEntity(entry.Entity.GetType().Name);
                }
            }
            else if (entry.State is EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;

                var modifiedProps = entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => p.Metadata.Name);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.UpdatingEntity(entry.Entity.GetType().Name, string.Join(", ", modifiedProps));
                }
            }
            else if (entry.State is EntityState.Deleted && logger.IsEnabled(LogLevel.Information))
            {
                logger.DeletingEntity(entry.Entity.GetType().Name);
            }
        }
    }
}
