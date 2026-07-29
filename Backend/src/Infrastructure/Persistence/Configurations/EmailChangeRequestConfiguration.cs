using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class EmailChangeRequestConfiguration : IEntityTypeConfiguration<EmailChangeRequest>
{
    public void Configure(EntityTypeBuilder<EmailChangeRequest> builder)
    {
        builder.ToTable("email_change_requests");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.NewEmail)
            .HasColumnName("new_email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Token)
            .HasColumnName("token")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(e => e.Token).IsUnique();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(e => e.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(u => u.EmailChangeRequests)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
