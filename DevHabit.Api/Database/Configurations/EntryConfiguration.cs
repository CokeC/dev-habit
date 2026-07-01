using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(500);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.Habit)
            .WithMany()
            .HasForeignKey(e => e.HabitId);
    }
}