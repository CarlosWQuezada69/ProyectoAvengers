using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProyectoAvengers.Domain.Entities;

namespace ProyectoAvengers.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.Sku)
            .HasColumnName("sku")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.Sku).IsUnique();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Slug)
            .HasColumnName("slug")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(e => e.Slug).IsUnique();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(e => e.Price)
            .HasColumnName("price")
            .HasColumnType("numeric(12,2)")
            .IsRequired();

        builder.Property(e => e.CompareAtPrice)
            .HasColumnName("compare_at_price")
            .HasColumnType("numeric(12,2)");

        builder.Property(e => e.Stock)
            .HasColumnName("stock")
            .HasDefaultValue(0);

        builder.Property(e => e.CategoryId)
            .HasColumnName("category_id");

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(e => e.IsFeatured)
            .HasColumnName("is_featured")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(e => e.RowVersion)
            .HasColumnName("row_version")
            .HasColumnType("bytea")
            .IsConcurrencyToken();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(e => e.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.CategoryId)
            .HasDatabaseName("idx_products_category");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("idx_products_active")
            .HasFilter("\"deleted_at\" IS NULL");
    }
}
