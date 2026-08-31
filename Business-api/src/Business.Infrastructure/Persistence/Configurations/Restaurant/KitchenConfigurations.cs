using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations.Restaurant;

public sealed class KitchenOrderConfiguration : IEntityTypeConfiguration<KitchenOrder>
{
    public void Configure(EntityTypeBuilder<KitchenOrder> builder)
    {
        builder.ToTable(
            "restaurant_kitchen_orders",
            table => table.HasCheckConstraint(
                "chk_kitchen_orders_status",
                "Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled')"));
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedOnAdd();
        builder.Property(order => order.KitchenNo).HasMaxLength(50).IsRequired();
        builder.Property(order => order.Status).HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(order => order.SentToKitchenDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(order => order.Note).HasMaxLength(500);
        builder.Property(order => order.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(order => order.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(order => order.KitchenNo)
            .IsUnique()
            .HasDatabaseName("uk_kitchen_orders_kitchen_no");
        builder.HasIndex(order => order.OrderId)
            .HasDatabaseName("ix_kitchen_orders_order");
        builder.HasIndex(order => new { order.Status, order.SentToKitchenDate })
            .HasDatabaseName("ix_kitchen_orders_status_sent");
        builder.HasOne(order => order.Order)
            .WithMany(source => source.KitchenOrders)
            .HasForeignKey(order => order.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_kitchen_orders_order");
    }
}

public sealed class KitchenOrderItemConfiguration : IEntityTypeConfiguration<KitchenOrderItem>
{
    public void Configure(EntityTypeBuilder<KitchenOrderItem> builder)
    {
        builder.ToTable(
            "restaurant_kitchen_order_items",
            table =>
            {
                table.HasCheckConstraint("chk_kitchen_order_items_quantity", "Quantity > 0");
                table.HasCheckConstraint(
                    "chk_kitchen_order_items_status",
                    "Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled')");
            });
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedOnAdd();
        builder.Property(item => item.Quantity).HasPrecision(18, 3);
        builder.Property(item => item.Status).HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(item => item.Note).HasMaxLength(500);
        builder.Property(item => item.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(item => item.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(item => new { item.KitchenOrderId, item.OrderItemId })
            .IsUnique()
            .HasDatabaseName("uk_kitchen_order_items_pair");
        builder.HasIndex(item => item.Status)
            .HasDatabaseName("ix_kitchen_order_items_status");
        builder.HasIndex(item => item.OrderItemId)
            .HasDatabaseName("ix_kitchen_order_items_order_item");
        builder.HasOne(item => item.KitchenOrder)
            .WithMany(order => order.Items)
            .HasForeignKey(item => item.KitchenOrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_kitchen_order_items_kitchen_order");
        builder.HasOne(item => item.OrderItem)
            .WithMany(orderItem => orderItem.KitchenOrderItems)
            .HasForeignKey(item => item.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_kitchen_order_items_order_item");
    }
}

public sealed class OrderItemKitchenQuantityConfiguration
    : IEntityTypeConfiguration<OrderItemKitchenQuantity>
{
    public void Configure(EntityTypeBuilder<OrderItemKitchenQuantity> builder)
    {
        builder.HasNoKey();
        builder.ToView("restaurant_order_item_kitchen_quantities");
        builder.Property(quantity => quantity.OrderedQuantity).HasPrecision(18, 3);
        builder.Property(quantity => quantity.SentQuantity).HasPrecision(18, 3);
        builder.Property(quantity => quantity.RemainingQuantity).HasPrecision(18, 3);
    }
}
