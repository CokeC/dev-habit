using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id).HasMaxLength(500);
        builder.Property(e => e.Email).HasMaxLength(300);
        builder.Property(e => e.IdentityId).HasMaxLength(500);
        builder.Property(e =>e.Name).HasMaxLength(100);

        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.IdentityId).IsUnique();
    }
}