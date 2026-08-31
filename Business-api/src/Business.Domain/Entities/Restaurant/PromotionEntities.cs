namespace Business.Domain.Entities.Restaurant;

public sealed class PromotionCode
{
    public ulong Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; } = true;
    public ulong? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public ulong? UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }
    public ICollection<PromotionFood> PromotionFoods { get; set; } = [];
    public ICollection<PromotionCategory> PromotionCategories { get; set; } = [];
    public ICollection<OrderPromotion> OrderPromotions { get; set; } = [];
}

public sealed class PromotionFood
{
    public ulong PromotionId { get; set; }
    public ulong FoodId { get; set; }
    public required PromotionCode Promotion { get; set; }
    public required Food Food { get; set; }
}

public sealed class PromotionCategory
{
    public ulong PromotionId { get; set; }
    public ulong CategoryId { get; set; }
    public required PromotionCode Promotion { get; set; }
    public required Category Category { get; set; }
}
