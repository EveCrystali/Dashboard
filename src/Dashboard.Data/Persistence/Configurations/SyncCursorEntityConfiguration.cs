using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard.Data.Persistence.Configurations;

public sealed class SyncCursorEntityConfiguration : IEntityTypeConfiguration<SyncCursorEntity>
{
    public void Configure(EntityTypeBuilder<SyncCursorEntity> builder)
    {
        builder.ToTable("SyncCursors");
        builder.HasKey(x => x.DataSourceId);
        builder.Property(x => x.DataSourceId).HasMaxLength(64);
    }
}
