using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations.Restaurant;

public sealed class PromotionCodeConfiguration : IEntityTypeConfiguration<PromotionCode>
{
    public void Configure(EntityTypeBuilder<PromotionCode> builder)
    {
        builder.ToTable(
            "restaurant_promotion_codes",
            table =>
            {
                table.HasCheckConstraint(
                    "chk_promotion_codes_type",
                    "DiscountType IN ('Percentage','FixedAmount')");
                table.HasCheckConstraint("chk_promotion_codes_value", "DiscountValue > 0");
                table.HasCheckConstraint("chk_promotion_codes_dates", "EndDate > StartDate");
                table.HasCheckConstraint(
                    "chk_promotion_codes_usage",
                    "UsageLimit IS NULL OR UsageLimit >= 0");
                table.HasCheckConstraint("chk_promotion_codes_usage_count", "UsageCount >= 0");
            });
        builder.HasKey(promotion => promotion.Id);
        builder.Property(promotion => promotion.Id).ValueGeneratedOnAdd();
        builder.Property(promotion => promotion.Code).HasMaxLength(50).IsRequired();
        builder.Property(promotion => promotion.Name).HasMaxLength(200).IsRequired();
        builder.Property(promotion => promotion.Description).HasMaxLength(1000);
        builder.Property(promotion => promotion.DiscountType).HasMaxLength(30).IsRequired();
        builder.Property(promotion => promotion.DiscountValue).HasPrecision(18, 2);
        builder.Property(promotion => promotion.MinOrderAmount).HasPrecision(18, 2);
        builder.Property(promotion => promotion.MaxDiscountAmount).HasPrecision(18, 2);
        builder.Property(promotion => promotion.UsageCount).HasDefaultValue(0);
        builder.Property(promotion => promotion.IsActive).HasDefaultValue(true);
        builder.Property(promotion => promotion.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(promotion => promotion.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(promotion => promotion.Code)
            .IsUnique()
            .HasDatabaseName("uk_promotion_codes_code");
        builder.HasIndex(
                promotion => new
                {
                    promotion.IsActive,
                    promotion.StartDate,
                    promotion.EndDate
                })
            .HasDatabaseName("ix_promotion_codes_validity");
    }
}

public sealed class PromotionFoodConfiguration : IEntityTypeConfiguration<PromotionFood>
{
    public void Configure(EntityTypeBuilder<PromotionFood> builder)
    {
        builder.ToTable("restaurant_promotion_foods");
        builder.HasKey(item => new { item.PromotionId, item.FoodId });
        builder.HasIndex(item => item.FoodId)
            .HasDatabaseName("ix_promotion_foods_food");
        builder.HasOne(item => item.Promotion)
            .WithMany(promotion => promotion.PromotionFoods)
            .HasForeignKey(item => item.PromotionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_promotion_foods_promotion");
        builder.HasOne(item => item.Food)
            .WithMany(food => food.PromotionFoods)
            .HasForeignKey(item => item.FoodId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_promotion_foods_food");
    }
}

public sealed class PromotionCategoryConfiguration : IEntityTypeConfiguration<PromotionCategory>
{
    public void Configure(EntityTypeBuilder<PromotionCategory> builder)
    {
        builder.ToTable("restaurant_promotion_categories");
        builder.HasKey(item => new { item.PromotionId, item.CategoryId });
        builder.HasIndex(item => item.CategoryId)
            .HasDatabaseName("ix_promotion_categories_category");
        builder.HasOne(item => item.Promotion)
            .WithMany(promotion => promotion.PromotionCategories)
            .HasForeignKey(item => item.PromotionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_promotion_categories_promotion");
        builder.HasOne(item => item.Category)
            .WithMany(category => category.PromotionCategories)
            .HasForeignKey(item => item.CategoryId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_promotion_categories_category");
    }
}
