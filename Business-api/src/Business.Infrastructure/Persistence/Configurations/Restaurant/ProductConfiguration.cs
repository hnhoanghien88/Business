using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations.Restaurant;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("restaurant_products");
        builder.HasKey(product => product.Code);

        builder.Property(product => product.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(255)
            .IsRequired();
    }
}
