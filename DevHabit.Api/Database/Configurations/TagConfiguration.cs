using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(500);
        builder.Property(e =>e.Name).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.HasIndex(e => new { e.Name }).IsUnique();
    }
}