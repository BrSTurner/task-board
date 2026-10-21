using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskBoard.Domain.Entities;

namespace TaskBoard.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title).IsRequired().HasMaxLength(TaskItem.TitleMaxLength);
        builder.Property(t => t.Description).HasMaxLength(TaskItem.DescriptionMaxLength);
        // Stored as text ("Todo", "InProgress", "Done") so the database stays readable.
        builder.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.DueDateUtc).IsRequired();

        // Every task belongs to exactly one user. Restrict: deleting a user with tasks must be an explicit decision.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.UserId);
    }
}
