using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class PageViewDailyConfiguration : IEntityTypeConfiguration<PageViewDaily>
{
    public void Configure(EntityTypeBuilder<PageViewDaily> builder)
    {
        builder.ToTable("page_view_daily");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.PageKey)
            .HasColumnName("page_key")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(e => e.Date)
            .HasColumnName("date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.Views)
            .HasColumnName("views")
            .HasDefaultValue(0);

        builder.HasIndex(e => new { e.PageKey, e.Date }).IsUnique();
    }
}