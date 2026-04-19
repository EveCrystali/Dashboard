using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard.Data.Persistence.Configurations;

public sealed class TodoEntityConfiguration : IEntityTypeConfiguration<TodoEntity>
{
    public void Configure(EntityTypeBuilder<TodoEntity> builder)
    {
        builder.ToTable("Todos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Priority).HasConversion<int?>();
        builder.Property(x => x.TagsJson).IsRequired();
        builder.Property(x => x.AssigneeIdsJson).IsRequired();
        builder.Property(x => x.SubtaskUrlsJson).IsRequired();
        builder.HasIndex(x => x.LastEditedTime);
    }
}
