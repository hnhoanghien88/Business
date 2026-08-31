namespace Business.Domain.Entities.Restaurant;

public sealed class Order
{
    public ulong Id { get; set; }
    public required string OrderNo { get; set; }
    public ulong? TableSessionId { get; set; }
    public ulong? CustomerId { get; set; }
    public string OrderType { get; set; } = "DineIn";
    public string Status { get; set; } = "Pending";
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public DateTime OrderedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public TableSession? TableSession { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderPromotion> Promotions { get; set; } = [];
    public ICollection<OrderStatusHistory> StatusHistories { get; set; } = [];
    public ICollection<KitchenOrder> KitchenOrders { get; set; } = [];
    public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = [];
}

public sealed class OrderItem
{
    public ulong Id { get; set; }
    public ulong OrderId { get; set; }
    public ulong FoodId { get; set; }
    public ulong FoodVariantId { get; set; }
    public required string FoodCode { get; set; }
    public required string FoodName { get; set; }
    public required string VariantName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required Order Order { get; set; }
    public required Food Food { get; set; }
    public required FoodVariant FoodVariant { get; set; }
    public ICollection<KitchenOrderItem> KitchenOrderItems { get; set; } = [];
}

public sealed class OrderPromotion
{
    public ulong Id { get; set; }
    public ulong OrderId { get; set; }
    public ulong PromotionId { get; set; }
    public required string PromotionCode { get; set; }
    public required string PromotionName { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedDate { get; set; }
    public required Order Order { get; set; }
    public required PromotionCode Promotion { get; set; }
}

public sealed class OrderStatusHistory
{
    public ulong Id { get; set; }
    public ulong OrderId { get; set; }
    public string? FromStatus { get; set; }
    public required string ToStatus { get; set; }
    public string? Note { get; set; }
    public ulong? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; }
    public required Order Order { get; set; }
}
