using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations.Restaurant;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("restaurant_categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedOnAdd();
        builder.Property(category => category.Code).HasMaxLength(50).IsRequired();
        builder.Property(category => category.Name).HasMaxLength(150).IsRequired();
        builder.Property(category => category.Description).HasMaxLength(500);
        builder.Property(category => category.DisplayOrder).HasDefaultValue(0);
        builder.Property(category => category.IsActive).HasDefaultValue(true);
        builder.Property(category => category.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(category => category.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(category => category.Code)
            .IsUnique()
            .HasDatabaseName("uk_categories_code");
        builder.HasIndex(category => new { category.IsActive, category.DisplayOrder })
            .HasDatabaseName("ix_categories_active_order");
        builder.HasIndex(category => new
            {
                category.ParentId,
                category.DisplayOrder,
                category.Name
            })
            .HasDatabaseName("ix_categories_parent_order");
        builder.HasOne(category => category.Parent)
            .WithMany(parent => parent.Children)
            .HasForeignKey(category => category.ParentId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_categories_parent");
    }
}

public sealed class FoodConfiguration : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.ToTable("restaurant_foods");
        builder.HasKey(food => food.Id);
        builder.Property(food => food.Id).ValueGeneratedOnAdd();
        builder.Property(food => food.Code).HasMaxLength(50).IsRequired();
        builder.Property(food => food.Name).HasMaxLength(200).IsRequired();
        builder.Property(food => food.Description).HasColumnType("text");
        builder.Property(food => food.ImageUrl).HasMaxLength(1000);
        builder.Property(food => food.DisplayOrder).HasDefaultValue(0);
        builder.Property(food => food.IsActive).HasDefaultValue(true);
        builder.Property(food => food.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(food => food.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(food => food.Code)
            .IsUnique()
            .HasDatabaseName("uk_foods_code");
        builder.HasIndex(food => new { food.CategoryId, food.IsActive, food.DisplayOrder })
            .HasDatabaseName("ix_foods_category_active");
        builder.HasOne(food => food.Category)
            .WithMany(category => category.Foods)
            .HasForeignKey(food => food.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_foods_category");
    }
}

public sealed class FoodVariantConfiguration : IEntityTypeConfiguration<FoodVariant>
{
    public void Configure(EntityTypeBuilder<FoodVariant> builder)
    {
        builder.ToTable(
            "restaurant_food_variants",
            table => table.HasCheckConstraint(
                "chk_food_variants_price",
                "CurrentPrice >= 0"));
        builder.HasKey(variant => variant.Id);
        builder.Property(variant => variant.Id).ValueGeneratedOnAdd();
        builder.Property(variant => variant.Code).HasMaxLength(50).IsRequired();
        builder.Property(variant => variant.Name).HasMaxLength(100).IsRequired();
        builder.Property(variant => variant.CurrentPrice).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(variant => variant.IsDefault).HasDefaultValue(false);
        builder.Property(variant => variant.IsAvailable).HasDefaultValue(true);
        builder.Property(variant => variant.SoldOutReason).HasMaxLength(500);
        builder.Property(variant => variant.DisplayOrder).HasDefaultValue(0);
        builder.Property(variant => variant.IsActive).HasDefaultValue(true);
        builder.Property(variant => variant.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(variant => variant.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(variant => new { variant.FoodId, variant.Code })
            .IsUnique()
            .HasDatabaseName("uk_food_variants_food_code");
        builder.HasIndex(
                variant => new
                {
                    variant.FoodId,
                    variant.IsActive,
                    variant.IsAvailable,
                    variant.DisplayOrder
                })
            .HasDatabaseName("ix_food_variants_food_available");
        builder.HasOne(variant => variant.Food)
            .WithMany(food => food.Variants)
            .HasForeignKey(variant => variant.FoodId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_food_variants_food");
    }
}

public sealed class FoodPriceHistoryConfiguration : IEntityTypeConfiguration<FoodPriceHistory>
{
    public void Configure(EntityTypeBuilder<FoodPriceHistory> builder)
    {
        builder.ToTable(
            "restaurant_food_price_histories",
            table =>
            {
                table.HasCheckConstraint("chk_food_price_histories_price", "Price >= 0");
                table.HasCheckConstraint(
                    "chk_food_price_histories_dates",
                    "EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom");
            });
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).ValueGeneratedOnAdd();
        builder.Property(history => history.Price).HasPrecision(18, 2);
        builder.Property(history => history.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.HasIndex(
                history => new
                {
                    history.FoodVariantId,
                    history.EffectiveFrom,
                    history.EffectiveTo
                })
            .HasDatabaseName("ix_food_price_histories_variant_dates");
        builder.HasOne(history => history.FoodVariant)
            .WithMany(variant => variant.PriceHistories)
            .HasForeignKey(history => history.FoodVariantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_food_price_histories_variant");
    }
}
