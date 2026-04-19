using Dashboard.Data.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dashboard.Data.Persistence.Configurations;

public sealed class JobApplicationEntityConfiguration : IEntityTypeConfiguration<JobApplicationEntity>
{
    public void Configure(EntityTypeBuilder<JobApplicationEntity> builder)
    {
        builder.ToTable("JobApplications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(64);
        builder.Property(x => x.Company).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Interest).HasConversion<int?>();
        builder.Property(x => x.PositionsJson).IsRequired();
        builder.Property(x => x.CompanyTypesJson).IsRequired();
        builder.Property(x => x.ContactMethodsJson).IsRequired();
        builder.Property(x => x.CvFileIdsJson).IsRequired();
        builder.Property(x => x.CoverLetterFileIdsJson).IsRequired();
        builder.HasIndex(x => x.LastEditedTime);
    }
}
