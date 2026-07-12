using HomeLabCore.Application.Interfaces.Database;
using HomeLabCore.Domain.Entities.Media;
using Microsoft.EntityFrameworkCore;

namespace HomeLabCore.Infrastructure.Database;

internal sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : DbContext(options), IApplicationDbContext
{
    public DbSet<MediaSearchSnapshot> MediaSearchSnapshots => Set<MediaSearchSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
