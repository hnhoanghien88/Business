using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Business.Infrastructure.Migrations.Restaurant
{
    /// <inheritdoc />
    public partial class ConsolidateProductsIntoFoods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "fk_categories_parent",
                        column: x => x.ParentId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "promotion_codes",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    DiscountType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaxDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_codes", x => x.Id);
                    table.CheckConstraint("chk_promotion_codes_dates", "EndDate > StartDate");
                    table.CheckConstraint("chk_promotion_codes_type", "DiscountType IN ('Percentage','FixedAmount')");
                    table.CheckConstraint("chk_promotion_codes_usage", "UsageLimit IS NULL OR UsageLimit >= 0");
                    table.CheckConstraint("chk_promotion_codes_usage_count", "UsageCount >= 0");
                    table.CheckConstraint("chk_promotion_codes_value", "DiscountValue > 0");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "restaurant_areas",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurant_areas", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "foods",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_foods", x => x.Id);
                    table.ForeignKey(
                        name: "fk_foods_category",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.Sql(
                """
                DROP PROCEDURE IF EXISTS sp_get_restaurant_products;
                DROP VIEW IF EXISTS restaurant_products_view;

                INSERT INTO categories
                    (Code, Name, Description, DisplayOrder, IsActive, CreatedDate, UpdatedDate)
                SELECT 'LEGACY', 'Chưa phân loại',
                       'Danh mục hệ thống dùng để tiếp nhận dữ liệu Product cũ.',
                       0, TRUE, CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)
                WHERE EXISTS (SELECT 1 FROM restaurant_products)
                  AND NOT EXISTS (SELECT 1 FROM categories WHERE Code = 'LEGACY');

                INSERT INTO foods
                    (CategoryId, Code, Name, DisplayOrder, IsActive, CreatedDate, UpdatedDate)
                SELECT c.Id, p.Code, LEFT(p.Name, 200), 0, TRUE,
                       CURRENT_TIMESTAMP(6), CURRENT_TIMESTAMP(6)
                FROM restaurant_products p
                JOIN categories c ON c.Code = 'LEGACY'
                WHERE NOT EXISTS (SELECT 1 FROM foods f WHERE f.Code = p.Code);
                """);

            migrationBuilder.DropTable(
                name: "restaurant_products");

            migrationBuilder.CreateTable(
                name: "promotion_categories",
                columns: table => new
                {
                    PromotionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CategoryId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_categories", x => new { x.PromotionId, x.CategoryId });
                    table.ForeignKey(
                        name: "fk_promotion_categories_category",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_promotion_categories_promotion",
                        column: x => x.PromotionId,
                        principalTable: "promotion_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "restaurant_tables",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AreaId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Available"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurant_tables", x => x.Id);
                    table.CheckConstraint("chk_restaurant_tables_capacity", "Capacity > 0");
                    table.CheckConstraint("chk_restaurant_tables_status", "Status IN ('Available','Occupied','Reserved','Cleaning','Disabled')");
                    table.ForeignKey(
                        name: "fk_restaurant_tables_area",
                        column: x => x.AreaId,
                        principalTable: "restaurant_areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_variants",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FoodId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SoldOutReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_variants", x => x.Id);
                    table.CheckConstraint("chk_food_variants_price", "CurrentPrice >= 0");
                    table.ForeignKey(
                        name: "fk_food_variants_food",
                        column: x => x.FoodId,
                        principalTable: "foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "promotion_foods",
                columns: table => new
                {
                    PromotionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    FoodId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_foods", x => new { x.PromotionId, x.FoodId });
                    table.ForeignKey(
                        name: "fk_promotion_foods_food",
                        column: x => x.FoodId,
                        principalTable: "foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_promotion_foods_promotion",
                        column: x => x.PromotionId,
                        principalTable: "promotion_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "table_sessions",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TableId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Open"),
                    OpenedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    ClosedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OpenedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ClosedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_sessions", x => x.Id);
                    table.CheckConstraint("chk_table_sessions_guest_count", "GuestCount > 0");
                    table.CheckConstraint("chk_table_sessions_status", "Status IN ('Open','Closed','Cancelled')");
                    table.ForeignKey(
                        name: "fk_table_sessions_table",
                        column: x => x.TableId,
                        principalTable: "restaurant_tables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "food_price_histories",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FoodVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_food_price_histories", x => x.Id);
                    table.CheckConstraint("chk_food_price_histories_dates", "EffectiveTo IS NULL OR EffectiveTo > EffectiveFrom");
                    table.CheckConstraint("chk_food_price_histories_price", "Price >= 0");
                    table.ForeignKey(
                        name: "fk_food_price_histories_variant",
                        column: x => x.FoodVariantId,
                        principalTable: "food_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderNo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    TableSessionId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CustomerId = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    OrderType = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "DineIn"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    SubtotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    OrderedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                    table.CheckConstraint("chk_orders_amounts", "SubtotalAmount >= 0 AND DiscountAmount >= 0 AND TaxAmount >= 0 AND TotalAmount >= 0");
                    table.CheckConstraint("chk_orders_status", "Status IN ('Draft','Pending','Confirmed','Serving','Completed','Cancelled')");
                    table.CheckConstraint("chk_orders_type", "OrderType IN ('DineIn','TakeAway','Delivery')");
                    table.ForeignKey(
                        name: "fk_orders_table_session",
                        column: x => x.TableSessionId,
                        principalTable: "table_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    TableSessionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    PaymentMethod = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    TransactionNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.CheckConstraint("chk_payments_amount", "Amount > 0");
                    table.CheckConstraint("chk_payments_method", "PaymentMethod IN ('Cash','Card','BankTransfer','Momo','VNPay','Other')");
                    table.CheckConstraint("chk_payments_status", "Status IN ('Pending','Paid','Failed','Refunded','Cancelled')");
                    table.ForeignKey(
                        name: "fk_payments_table_session",
                        column: x => x.TableSessionId,
                        principalTable: "table_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "kitchen_orders",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    KitchenNo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    SentToKitchenDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    AcceptedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StartedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReadyDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kitchen_orders", x => x.Id);
                    table.CheckConstraint("chk_kitchen_orders_status", "Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled')");
                    table.ForeignKey(
                        name: "fk_kitchen_orders_order",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    FoodId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    FoodVariantId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    FoodCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    FoodName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    VariantName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.Id);
                    table.CheckConstraint("chk_order_items_amounts", "UnitPrice >= 0 AND DiscountAmount >= 0 AND TotalAmount >= 0");
                    table.CheckConstraint("chk_order_items_quantity", "Quantity > 0");
                    table.CheckConstraint("chk_order_items_status", "Status IN ('Pending','Accepted','Preparing','Ready','Served','Completed','Cancelled','Rejected')");
                    table.ForeignKey(
                        name: "fk_order_items_food",
                        column: x => x.FoodId,
                        principalTable: "foods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_items_order",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_items_variant",
                        column: x => x.FoodVariantId,
                        principalTable: "food_variants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_promotions",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    PromotionId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    PromotionCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    PromotionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_promotions", x => x.Id);
                    table.CheckConstraint("chk_order_promotions_discount", "DiscountAmount >= 0");
                    table.ForeignKey(
                        name: "fk_order_promotions_order",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_promotions_promotion",
                        column: x => x.PromotionId,
                        principalTable: "promotion_codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_status_histories",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    FromStatus = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    ChangedBy = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_histories", x => x.Id);
                    table.ForeignKey(
                        name: "fk_order_status_histories_order",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payment_allocations",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PaymentId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    OrderId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_allocations", x => x.Id);
                    table.CheckConstraint("chk_payment_allocations_amount", "Amount > 0");
                    table.ForeignKey(
                        name: "fk_payment_allocations_order",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_allocations_payment",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "kitchen_order_items",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    KitchenOrderId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    OrderItemId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    StartedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReadyDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kitchen_order_items", x => x.Id);
                    table.CheckConstraint("chk_kitchen_order_items_quantity", "Quantity > 0");
                    table.CheckConstraint("chk_kitchen_order_items_status", "Status IN ('Pending','Accepted','Preparing','Ready','Completed','Rejected','Cancelled')");
                    table.ForeignKey(
                        name: "fk_kitchen_order_items_kitchen_order",
                        column: x => x.KitchenOrderId,
                        principalTable: "kitchen_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_kitchen_order_items_order_item",
                        column: x => x.OrderItemId,
                        principalTable: "order_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_categories_active_order",
                table: "categories",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_order",
                table: "categories",
                columns: new[] { "ParentId", "DisplayOrder", "Name" });

            migrationBuilder.CreateIndex(
                name: "uk_categories_code",
                table: "categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_food_price_histories_variant_dates",
                table: "food_price_histories",
                columns: new[] { "FoodVariantId", "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "ix_food_variants_food_available",
                table: "food_variants",
                columns: new[] { "FoodId", "IsActive", "IsAvailable", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "uk_food_variants_food_code",
                table: "food_variants",
                columns: new[] { "FoodId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_foods_category_active",
                table: "foods",
                columns: new[] { "CategoryId", "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "uk_foods_code",
                table: "foods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kitchen_order_items_order_item",
                table: "kitchen_order_items",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "ix_kitchen_order_items_status",
                table: "kitchen_order_items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "uk_kitchen_order_items_pair",
                table: "kitchen_order_items",
                columns: new[] { "KitchenOrderId", "OrderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_kitchen_orders_order",
                table: "kitchen_orders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "ix_kitchen_orders_status_sent",
                table: "kitchen_orders",
                columns: new[] { "Status", "SentToKitchenDate" });

            migrationBuilder.CreateIndex(
                name: "uk_kitchen_orders_kitchen_no",
                table: "kitchen_orders",
                column: "KitchenNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_food",
                table: "order_items",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_status",
                table: "order_items",
                columns: new[] { "OrderId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_order_items_variant",
                table: "order_items",
                column: "FoodVariantId");

            migrationBuilder.CreateIndex(
                name: "ix_order_promotions_promotion",
                table: "order_promotions",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "uk_order_promotions_order_promotion",
                table: "order_promotions",
                columns: new[] { "OrderId", "PromotionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_status_histories_order_date",
                table: "order_status_histories",
                columns: new[] { "OrderId", "ChangedDate" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer",
                table: "orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "ix_orders_ordered_date_status",
                table: "orders",
                columns: new[] { "OrderedDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_table_session",
                table: "orders",
                column: "TableSessionId");

            migrationBuilder.CreateIndex(
                name: "uk_orders_order_no",
                table: "orders",
                column: "OrderNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_allocations_order",
                table: "payment_allocations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "uk_payment_allocations_payment_order",
                table: "payment_allocations",
                columns: new[] { "PaymentId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_paid_date_status",
                table: "payments",
                columns: new[] { "PaidDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_session_status",
                table: "payments",
                columns: new[] { "TableSessionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_transaction_no",
                table: "payments",
                column: "TransactionNo");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_categories_category",
                table: "promotion_categories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "ix_promotion_codes_validity",
                table: "promotion_codes",
                columns: new[] { "IsActive", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "uk_promotion_codes_code",
                table: "promotion_codes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promotion_foods_food",
                table: "promotion_foods",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_areas_active_order",
                table: "restaurant_areas",
                columns: new[] { "IsActive", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "uk_restaurant_areas_code",
                table: "restaurant_areas",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_tables_area_status",
                table: "restaurant_tables",
                columns: new[] { "AreaId", "Status", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "uk_restaurant_tables_code",
                table: "restaurant_tables",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_table_sessions_opened_date",
                table: "table_sessions",
                column: "OpenedDate");

            migrationBuilder.CreateIndex(
                name: "ix_table_sessions_table_status",
                table: "table_sessions",
                columns: new[] { "TableId", "Status" });
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/ConsolidateProductsIntoFoods/Up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/ConsolidateProductsIntoFoods/Down");

            migrationBuilder.CreateTable(
                name: "restaurant_products",
                columns: table => new
                {
                    Code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurant_products", x => x.Code);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.Sql(
                """
                INSERT INTO restaurant_products (Code, Name)
                SELECT Code, Name
                FROM foods;

                CREATE VIEW restaurant_products_view AS
                SELECT * FROM restaurant_products;

                CREATE PROCEDURE sp_get_restaurant_products()
                SELECT * FROM restaurant_products;
                """);

            migrationBuilder.DropTable(
                name: "food_price_histories");

            migrationBuilder.DropTable(
                name: "kitchen_order_items");

            migrationBuilder.DropTable(
                name: "order_promotions");

            migrationBuilder.DropTable(
                name: "order_status_histories");

            migrationBuilder.DropTable(
                name: "payment_allocations");

            migrationBuilder.DropTable(
                name: "promotion_categories");

            migrationBuilder.DropTable(
                name: "promotion_foods");

            migrationBuilder.DropTable(
                name: "kitchen_orders");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "promotion_codes");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "food_variants");

            migrationBuilder.DropTable(
                name: "table_sessions");

            migrationBuilder.DropTable(
                name: "foods");

            migrationBuilder.DropTable(
                name: "restaurant_tables");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "restaurant_areas");

        }
    }
}
