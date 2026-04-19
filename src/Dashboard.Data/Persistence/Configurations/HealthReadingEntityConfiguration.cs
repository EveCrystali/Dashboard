using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard.Data.Persistence.Configurations;

public sealed class HealthReadingEntityConfiguration : IEntityTypeConfiguration<HealthReadingEntity>
{
    public void Configure(EntityTypeBuilder<HealthReadingEntity> builder)
    {
        builder.ToTable("HealthReadings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.EntryType).HasConversion<int?>();
        builder.Property(x => x.Verdict).HasConversion<int?>();
        builder.Property(x => x.Source).HasConversion<int?>();
        builder.Property(x => x.ExerciseTypesJson).IsRequired();
        builder.HasIndex(x => x.DateStart);
        builder.HasIndex(x => x.LastEditedTime);
    }
}
