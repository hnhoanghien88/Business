namespace Business.Domain.Entities.Restaurant;

public sealed class KitchenOrder
{
    public ulong Id { get; set; }
    public ulong OrderId { get; set; }
    public required string KitchenNo { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime SentToKitchenDate { get; set; }
    public ulong? AcceptedBy { get; set; }
    public DateTime? AcceptedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? ReadyDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required Order Order { get; set; }
    public ICollection<KitchenOrderItem> Items { get; set; } = [];
}

public sealed class KitchenOrderItem
{
    public ulong Id { get; set; }
    public ulong KitchenOrderId { get; set; }
    public ulong OrderItemId { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime? StartedDate { get; set; }
    public DateTime? ReadyDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required KitchenOrder KitchenOrder { get; set; }
    public required OrderItem OrderItem { get; set; }
}

public sealed class OrderItemKitchenQuantity
{
    public ulong OrderItemId { get; set; }
    public ulong OrderId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal SentQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}
