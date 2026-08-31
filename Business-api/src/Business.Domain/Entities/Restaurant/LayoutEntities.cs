namespace Business.Domain.Entities.Restaurant;

public sealed class RestaurantArea
{
    public ulong Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public ICollection<RestaurantTable> Tables { get; set; } = [];
}

public sealed class RestaurantTable
{
    public ulong Id { get; set; }
    public ulong AreaId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required RestaurantArea Area { get; set; }
    public ICollection<TableSession> Sessions { get; set; } = [];
}

public sealed class TableSession
{
    public ulong Id { get; set; }
    public ulong TableId { get; set; }
    public int GuestCount { get; set; } = 1;
    public string Status { get; set; } = "Open";
    public DateTime OpenedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public ulong? OpenedBy { get; set; }
    public ulong? ClosedBy { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required RestaurantTable Table { get; set; }
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
}
