using DevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevHabit.Api.Database.Configurations;

public class HabitConfiguration : IEntityTypeConfiguration<Habit>
{
    public void Configure(EntityTypeBuilder<Habit> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasMaxLength(500);

        builder.Property(e => e.Name).HasMaxLength(100);

        builder.Property(e => e.Description).HasMaxLength(500);

        builder.OwnsOne(e => e.Frequency);

        builder.OwnsOne(e => e.Target, b =>
        {
            b.Property(t => t.Unit).HasMaxLength(100);
        });

        builder.OwnsOne(e => e.Milestone);

        builder.HasMany(e => e.Tags)
            .WithMany()
            .UsingEntity<HabitTag>();
    }
}
