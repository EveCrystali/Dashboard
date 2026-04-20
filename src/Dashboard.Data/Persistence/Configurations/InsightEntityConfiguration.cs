using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard.Data.Persistence.Configurations;

public sealed class InsightEntityConfiguration : IEntityTypeConfiguration<InsightEntity>
{
    public void Configure(EntityTypeBuilder<InsightEntity> builder)
    {
        builder.ToTable("Insights");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.RuleId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SnapshotId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Detail).IsRequired();
        builder.Property(x => x.ActionDeepLink).HasMaxLength(1024);
        builder.Property(x => x.Severity).HasConversion<int>();

        builder.HasIndex(x => x.SnapshotId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
