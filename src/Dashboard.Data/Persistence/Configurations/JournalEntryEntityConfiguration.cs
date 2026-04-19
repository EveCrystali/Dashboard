using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard.Data.Persistence.Configurations;

public sealed class JournalEntryEntityConfiguration : IEntityTypeConfiguration<JournalEntryEntity>
{
    public void Configure(EntityTypeBuilder<JournalEntryEntity> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Type).HasConversion<int?>();
        builder.Property(x => x.Source).HasConversion<int?>();
        builder.Property(x => x.DomainsJson).IsRequired();
        builder.HasIndex(x => x.CreatedTime);
        builder.HasIndex(x => x.LastEditedTime);
    }
}
