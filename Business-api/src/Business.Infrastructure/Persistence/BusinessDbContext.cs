using Business.Domain.Entities;
using Business.Domain.Entities.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public sealed class BusinessDbContext(DbContextOptions<BusinessDbContext> options)
    : DbContext(options)
{
    public DbSet<RestaurantArea> RestaurantAreas => Set<RestaurantArea>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<TableSession> TableSessions => Set<TableSession>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<FoodVariant> FoodVariants => Set<FoodVariant>();
    public DbSet<FoodPriceHistory> FoodPriceHistories => Set<FoodPriceHistory>();
    public DbSet<PromotionCode> PromotionCodes => Set<PromotionCode>();
    public DbSet<PromotionFood> PromotionFoods => Set<PromotionFood>();
    public DbSet<PromotionCategory> PromotionCategories => Set<PromotionCategory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderPromotion> OrderPromotions => Set<OrderPromotion>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<KitchenOrder> KitchenOrders => Set<KitchenOrder>();
    public DbSet<KitchenOrderItem> KitchenOrderItems => Set<KitchenOrderItem>();
    public DbSet<OrderItemKitchenQuantity> OrderItemKitchenQuantities => Set<OrderItemKitchenQuantity>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<OrderPaymentBalance> OrderPaymentBalances => Set<OrderPaymentBalance>();
    public DbSet<TableSessionPaymentBalance> TableSessionPaymentBalances => Set<TableSessionPaymentBalance>();
    public DbSet<RateLimitPolicy> RateLimitPolicies => Set<RateLimitPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BusinessDbContext).Assembly);
    }
}
