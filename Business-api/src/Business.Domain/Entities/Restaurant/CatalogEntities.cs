namespace Business.Domain.Entities.Restaurant;

public sealed class Category
{
    public ulong Id { get; set; }
    public ulong? ParentId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<Food> Foods { get; set; } = [];
    public ICollection<PromotionCategory> PromotionCategories { get; set; } = [];
}

public sealed class Food
{
    public ulong Id { get; set; }
    public ulong CategoryId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required Category Category { get; set; }
    public ICollection<FoodVariant> Variants { get; set; } = [];
    public ICollection<PromotionFood> PromotionFoods { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public sealed class FoodVariant
{
    public ulong Id { get; set; }
    public ulong FoodId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public decimal CurrentPrice { get; set; }
    public bool IsDefault { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? SoldOutReason { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public required Food Food { get; set; }
    public ICollection<FoodPriceHistory> PriceHistories { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public sealed class FoodPriceHistory
{
    public ulong Id { get; set; }
    public ulong FoodVariantId { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public required FoodVariant FoodVariant { get; set; }
}
