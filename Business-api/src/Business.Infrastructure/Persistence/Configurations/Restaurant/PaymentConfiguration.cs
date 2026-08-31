using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations.Restaurant;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable(
            "restaurant_payments",
            table =>
            {
                table.HasCheckConstraint(
                    "chk_payments_method",
                    "PaymentMethod IN ('Cash','Card','BankTransfer','Momo','VNPay','Other')");
                table.HasCheckConstraint(
                    "chk_payments_status",
                    "Status IN ('Pending','Paid','Failed','Refunded','Cancelled')");
                table.HasCheckConstraint("chk_payments_amount", "Amount > 0");
            });
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedOnAdd();
        builder.Property(payment => payment.PaymentMethod).HasMaxLength(30).IsRequired();
        builder.Property(payment => payment.Amount).HasPrecision(18, 2);
        builder.Property(payment => payment.Status).HasMaxLength(30).HasDefaultValue("Pending");
        builder.Property(payment => payment.TransactionNo).HasMaxLength(100);
        builder.Property(payment => payment.Note).HasMaxLength(500);
        builder.Property(payment => payment.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(payment => payment.UpdatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
        builder.HasIndex(payment => new { payment.TableSessionId, payment.Status })
            .HasDatabaseName("ix_payments_session_status");
        builder.HasIndex(payment => new { payment.PaidDate, payment.Status })
            .HasDatabaseName("ix_payments_paid_date_status");
        builder.HasIndex(payment => payment.TransactionNo)
            .HasDatabaseName("ix_payments_transaction_no");
        builder.HasOne(payment => payment.TableSession)
            .WithMany(session => session.Payments)
            .HasForeignKey(payment => payment.TableSessionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_payments_table_session");
    }
}

public sealed class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable(
            "restaurant_payment_allocations",
            table => table.HasCheckConstraint(
                "chk_payment_allocations_amount",
                "Amount > 0"));
        builder.HasKey(allocation => allocation.Id);
        builder.Property(allocation => allocation.Id).ValueGeneratedOnAdd();
        builder.Property(allocation => allocation.Amount).HasPrecision(18, 2);
        builder.Property(allocation => allocation.CreatedDate)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.HasIndex(allocation => new { allocation.PaymentId, allocation.OrderId })
            .IsUnique()
            .HasDatabaseName("uk_payment_allocations_payment_order");
        builder.HasIndex(allocation => allocation.OrderId)
            .HasDatabaseName("ix_payment_allocations_order");
        builder.HasOne(allocation => allocation.Payment)
            .WithMany(payment => payment.Allocations)
            .HasForeignKey(allocation => allocation.PaymentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_payment_allocations_payment");
        builder.HasOne(allocation => allocation.Order)
            .WithMany(order => order.PaymentAllocations)
            .HasForeignKey(allocation => allocation.OrderId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_payment_allocations_order");
    }
}

public sealed class OrderPaymentBalanceConfiguration : IEntityTypeConfiguration<OrderPaymentBalance>
{
    public void Configure(EntityTypeBuilder<OrderPaymentBalance> builder)
    {
        builder.HasNoKey();
        builder.ToView("restaurant_order_payment_balances");
        builder.Property(balance => balance.TotalAmount).HasPrecision(18, 2);
        builder.Property(balance => balance.PaidAmount).HasPrecision(18, 2);
        builder.Property(balance => balance.RemainingAmount).HasPrecision(18, 2);
    }
}

public sealed class TableSessionPaymentBalanceConfiguration
    : IEntityTypeConfiguration<TableSessionPaymentBalance>
{
    public void Configure(EntityTypeBuilder<TableSessionPaymentBalance> builder)
    {
        builder.HasNoKey();
        builder.ToView("restaurant_table_session_payment_balances");
        builder.Property(balance => balance.TotalAmount).HasPrecision(18, 2);
        builder.Property(balance => balance.PaidAmount).HasPrecision(18, 2);
        builder.Property(balance => balance.RemainingAmount).HasPrecision(18, 2);
    }
}
