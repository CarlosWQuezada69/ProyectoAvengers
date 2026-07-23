using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class SliderItemConfiguration : IEntityTypeConfiguration<SliderItem>
{
    public void Configure(EntityTypeBuilder<SliderItem> builder)
    {
        builder.ToTable("slider_items");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Subtitle)
            .HasColumnName("subtitle")
            .HasMaxLength(300);

        builder.Property(e => e.ImageUrl)
            .HasColumnName("image_url")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.LinkUrl)
            .HasColumnName("link_url")
            .HasColumnType("text");

        builder.Property(e => e.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(e => e.StartsAt)
            .HasColumnName("starts_at");

        builder.Property(e => e.EndsAt)
            .HasColumnName("ends_at");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
