using HomeLabCore.Domain.Entities.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeLabCore.Infrastructure.Database.Configurations;

internal sealed class MediaSearchSnapshotConfiguration : IEntityTypeConfiguration<MediaSearchSnapshot>
{
    public void Configure(EntityTypeBuilder<MediaSearchSnapshot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Query)
            .IsRequired()
            .HasMaxLength(255);

        builder.OwnsMany(x => x.Results, resultsBuilder =>
        {
            resultsBuilder.ToJson();
        });
    }
}
