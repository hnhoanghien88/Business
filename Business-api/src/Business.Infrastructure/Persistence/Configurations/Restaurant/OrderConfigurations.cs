using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations.Restaurant;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(
            "restaurant_orders",
            table =>
            {
                table.HasCheckConstraint(
                    "chk_orders_type",
                    "OrderType IN ('DineIn','TakeAway','Delivery')");
                table.HasCheckConstraint(
                    "chk_orders_status",
                    "Status IN ('Draft','Pending','Confirmed','Serving','Completed','Cancelled')");
                table.HasCheckConstraint(
                    "chk_orders_amounts",
                    "SubtotalAmount >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND TotalAmount >= 0");
            });
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedOnAdd();
        builder.Property(order => order.OrderNo).HasMaxLength(50).IsRequired();
        builder.Property(order => order.OrderType).HasMaxLength(30).HasDefaultValue("DineIn");
        builder.Property(order => order.Status).HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(order => order.SubtotalAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(order => order.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(order => order.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(order => order.TotalAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(order => order.Note).HasMaxLength(1000);
        builder.Property(order => order.OrderedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(order => order.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(order => order.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(order => order.OrderNo)
            .IsUnique()
            .HasDatabaseName("uk_orders_order_no");
        builder.HasIndex(order => new { order.OrderedDate, order.Status })
            .HasDatabaseName("ix_orders_ordered_date_status");
        builder.HasIndex(order => order.TableSessionId)
            .HasDatabaseName("ix_orders_table_session");
        builder.HasIndex(order => order.CustomerId)
            .HasDatabaseName("ix_orders_customer");
        builder.HasOne(order => order.TableSession)
            .WithMany(session => session.Orders)
            .HasForeignKey(order => order.TableSessionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_orders_table_session");
    }
}

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            "restaurant_order_items",
            table =>
            {
                table.HasCheckConstraint("chk_order_items_quantity", "Quantity > 0");
                table.HasCheckConstraint(
                    "chk_order_items_amounts",
                    "UnitPrice >= 0 AND DiscountAmount >= 0 AND TotalAmount >= 0");
                table.HasCheckConstraint(
                    "chk_order_items_status",
                    "Status IN ('Pending','Accepted','Preparing','Ready','Served','Completed','Cancelled','Rejected')");
            });
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedOnAdd();
        builder.Property(item => item.FoodCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.FoodName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.VariantName).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Quantity).HasPrecision(18, 3);
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        builder.Property(item => item.TotalAmount).HasPrecision(18, 2);
        builder.Property(item => item.Note).HasMaxLength(500);
        builder.Property(item => item.Status).HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(item => item.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(item => item.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(item => new { item.OrderId, item.Status })
            .HasDatabaseName("ix_order_items_order_status");
        builder.HasIndex(item => item.FoodId).HasDatabaseName("ix_order_items_food");
        builder.HasIndex(item => item.FoodVariantId).HasDatabaseName("ix_order_items_variant");
        builder.HasOne(item => item.Order)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_order_items_order");
        builder.HasOne(item => item.Food)
            .WithMany(food => food.OrderItems)
            .HasForeignKey(item => item.FoodId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_order_items_food");
        builder.HasOne(item => item.FoodVariant)
            .WithMany(variant => variant.OrderItems)
            .HasForeignKey(item => item.FoodVariantId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_order_items_variant");
    }
}

public sealed class OrderPromotionConfiguration : IEntityTypeConfiguration<OrderPromotion>
{
    public void Configure(EntityTypeBuilder<OrderPromotion> builder)
    {
        builder.ToTable(
            "restaurant_order_promotions",
            table => table.HasCheckConstraint(
                "chk_order_promotions_discount",
                "DiscountAmount >= 0"));
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedOnAdd();
        builder.Property(item => item.PromotionCode).HasMaxLength(50).IsRequired();
        builder.Property(item => item.PromotionName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.DiscountAmount).HasPrecision(18, 2);
        builder.Property(item => item.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.HasIndex(item => new { item.OrderId, item.PromotionId })
            .IsUnique()
            .HasDatabaseName("uk_order_promotions_order_promotion");
        builder.HasIndex(item => item.PromotionId)
            .HasDatabaseName("ix_order_promotions_promotion");
        builder.HasOne(item => item.Order)
            .WithMany(order => order.Promotions)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_order_promotions_order");
        builder.HasOne(item => item.Promotion)
            .WithMany(promotion => promotion.OrderPromotions)
            .HasForeignKey(item => item.PromotionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_order_promotions_promotion");
    }
}

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("restaurant_order_status_histories");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).ValueGeneratedOnAdd();
        builder.Property(history => history.FromStatus).HasMaxLength(30);
        builder.Property(history => history.ToStatus).HasMaxLength(30).IsRequired();
        builder.Property(history => history.Note).HasMaxLength(500);
        builder.Property(history => history.ChangedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.HasIndex(history => new { history.OrderId, history.ChangedDate })
            .HasDatabaseName("ix_order_status_histories_order_date");
        builder.HasOne(history => history.Order)
            .WithMany(order => order.StatusHistories)
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_order_status_histories_order");
    }
}
