using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.ToTable("site_settings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Key)
            .HasColumnName("key")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(e => e.Key).IsUnique();

        builder.Property(e => e.Value)
            .HasColumnName("value")
            .HasColumnType("text");

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.UpdatedByUserId)
            .HasColumnName("updated_by_user_id");

        builder.HasOne(e => e.UpdatedByUser)
            .WithMany()
            .HasForeignKey(e => e.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
