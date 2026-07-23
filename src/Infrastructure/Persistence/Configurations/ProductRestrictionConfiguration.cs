using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class ProductRestrictionConfiguration : IEntityTypeConfiguration<ProductRestriction>
{
    public void Configure(EntityTypeBuilder<ProductRestriction> builder)
    {
        builder.ToTable("product_restrictions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.ProductId)
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(e => e.RestrictionType)
            .HasColumnName("restriction_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Config)
            .HasColumnName("config")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();

        builder.Property(e => e.StartsAt)
            .HasColumnName("starts_at");

        builder.Property(e => e.EndsAt)
            .HasColumnName("ends_at");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.HasOne(e => e.Product)
            .WithMany(p => p.ProductRestrictions)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.ProductId)
            .HasDatabaseName("idx_product_restrictions_product");

        builder.HasIndex(e => e.Config)
            .HasDatabaseName("idx_product_restrictions_config_gin")
            .HasMethod("GIN");
    }
}
