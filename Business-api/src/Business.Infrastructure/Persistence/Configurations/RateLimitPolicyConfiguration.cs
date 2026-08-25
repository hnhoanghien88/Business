using Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations;

public sealed class RateLimitPolicyConfiguration : IEntityTypeConfiguration<RateLimitPolicy>
{
    public void Configure(EntityTypeBuilder<RateLimitPolicy> builder)
    {
        builder.ToTable("rate_limit_policies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RoutePattern).HasMaxLength(255).IsRequired();
        builder.Property(x => x.HttpMethods).HasMaxLength(100);
        builder.Property(x => x.PartitionBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Algorithm).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
