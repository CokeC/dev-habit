using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public class EntryImportJobConfiguration : IEntityTypeConfiguration<EntryImportJob>
{
    public void Configure(EntityTypeBuilder<EntryImportJob> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(500);
    }
}
