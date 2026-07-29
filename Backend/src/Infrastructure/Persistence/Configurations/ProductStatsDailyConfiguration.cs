using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class ProductStatsDailyConfiguration : IEntityTypeConfiguration<ProductStatsDaily>
{
    public void Configure(EntityTypeBuilder<ProductStatsDaily> builder)
    {
        builder.ToTable("product_stats_daily");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(e => e.Date)
            .HasColumnName("date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.Views)
            .HasColumnName("views")
            .HasDefaultValue(0);

        builder.Property(e => e.Purchases)
            .HasColumnName("purchases")
            .HasDefaultValue(0);

        builder.HasOne(e => e.Product)
            .WithMany(p => p.ProductStatsDailies)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ProductId, e.Date }).IsUnique();
    }
}
