namespace Business.Domain.Entities.Restaurant;

public sealed class Payment
{
    public ulong Id { get; set; }
    public ulong TableSessionId { get; set; }
    public required string PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string? TransactionNo { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Note { get; set; }
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required TableSession TableSession { get; set; }
    public ICollection<PaymentAllocation> Allocations { get; set; } = [];
}

public sealed class PaymentAllocation
{
    public ulong Id { get; set; }
    public ulong PaymentId { get; set; }
    public ulong OrderId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public required Payment Payment { get; set; }
    public required Order Order { get; set; }
}

public sealed class OrderPaymentBalance
{
    public ulong OrderId { get; set; }
    public ulong? TableSessionId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
}

public sealed class TableSessionPaymentBalance
{
    public ulong TableSessionId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
}
