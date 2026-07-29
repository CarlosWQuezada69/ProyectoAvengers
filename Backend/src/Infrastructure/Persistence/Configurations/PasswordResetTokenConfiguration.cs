using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.Token)
            .HasColumnName("token")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(e => e.Token).IsUnique();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(e => e.UsedAt)
            .HasColumnName("used_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(u => u.PasswordResetTokens)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
