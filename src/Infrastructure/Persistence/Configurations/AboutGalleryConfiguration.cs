using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class AboutGalleryConfiguration : IEntityTypeConfiguration<AboutGallery>
{
    public void Configure(EntityTypeBuilder<AboutGallery> builder)
    {
        builder.ToTable("about_gallery");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.AboutInfoId)
            .HasColumnName("about_info_id")
            .IsRequired();

        builder.Property(e => e.Url)
            .HasColumnName("url")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(200);

        builder.Property(e => e.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(e => e.Section)
            .HasColumnName("section")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(e => e.AboutInfo)
            .WithMany(a => a.Gallery)
            .HasForeignKey(e => e.AboutInfoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}