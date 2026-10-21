using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskBoard.Domain.Entities;
using TaskBoard.Infrastructure.Persistence.Seeding;

namespace TaskBoard.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        // SQLite does not enforce these lengths; the domain and validators do. They document the schema.
        builder.Property(u => u.Name).IsRequired().HasMaxLength(User.NameMaxLength);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(User.EmailMaxLength);
        builder.Property(u => u.TimeZone).IsRequired().HasMaxLength(User.TimeZoneMaxLength);
        builder.Property(u => u.PasswordHash).IsRequired();

        // Emails are stored normalized (lower-case), so a plain unique index is case-insensitive in practice.
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasData(SeedData.Users);
    }
}
