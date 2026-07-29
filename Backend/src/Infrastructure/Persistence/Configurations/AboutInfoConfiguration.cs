using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class AboutInfoConfiguration : IEntityTypeConfiguration<AboutInfo>
{
    public void Configure(EntityTypeBuilder<AboutInfo> builder)
    {
        builder.ToTable("about_info");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.History)
            .HasColumnName("history")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(e => e.Mission)
            .HasColumnName("mission")
            .HasColumnType("text");

        builder.Property(e => e.Vision)
            .HasColumnName("vision")
            .HasColumnType("text");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");
    }
}