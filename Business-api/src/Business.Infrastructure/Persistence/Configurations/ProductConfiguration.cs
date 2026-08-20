using Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("product");
        builder.HasKey(product => product.Code);

        builder.Property(product => product.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(255)
            .IsRequired();
    }
}
