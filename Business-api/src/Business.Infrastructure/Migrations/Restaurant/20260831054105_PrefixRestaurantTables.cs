using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business.Infrastructure.Migrations.Restaurant;

/// <inheritdoc />
public partial class PrefixRestaurantTables : Migration
{
    private static readonly (string OldName, string NewName)[] TableRenames =
    [
        ("categories", "restaurant_categories"),
        ("food_price_histories", "restaurant_food_price_histories"),
        ("food_variants", "restaurant_food_variants"),
        ("foods", "restaurant_foods"),
        ("kitchen_order_items", "restaurant_kitchen_order_items"),
        ("kitchen_orders", "restaurant_kitchen_orders"),
        ("order_items", "restaurant_order_items"),
        ("order_promotions", "restaurant_order_promotions"),
        ("order_status_histories", "restaurant_order_status_histories"),
        ("orders", "restaurant_orders"),
        ("payment_allocations", "restaurant_payment_allocations"),
        ("payments", "restaurant_payments"),
        ("promotion_categories", "restaurant_promotion_categories"),
        ("promotion_codes", "restaurant_promotion_codes"),
        ("promotion_foods", "restaurant_promotion_foods"),
        ("rate_limit_policies", "restaurant_rate_limit_policies"),
        ("table_sessions", "restaurant_table_sessions")
    ];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var (oldName, newName) in TableRenames)
            migrationBuilder.RenameTable(name: oldName, newName: newName);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var (oldName, newName) in TableRenames.Reverse())
            migrationBuilder.RenameTable(name: newName, newName: oldName);
    }
}
