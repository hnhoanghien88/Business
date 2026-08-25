using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business.Infrastructure.Migrations.Restaurant
{
    /// <inheritdoc />
    public partial class RestaurantInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_get_products;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS product_view;");
            migrationBuilder.DropPrimaryKey(name: "PK_product", table: "product");
            migrationBuilder.RenameTable(name: "product", newName: "restaurant_products");
            migrationBuilder.AddPrimaryKey(name: "PK_restaurant_products", table: "restaurant_products", column: "Code");
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/RestaurantInitial/Up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/RestaurantInitial/Down");
            migrationBuilder.DropPrimaryKey(name: "PK_restaurant_products", table: "restaurant_products");
            migrationBuilder.RenameTable(name: "restaurant_products", newName: "product");
            migrationBuilder.AddPrimaryKey(name: "PK_product", table: "product", column: "Code");
            migrationBuilder.Sql("CREATE VIEW product_view AS SELECT * FROM product;");
            migrationBuilder.Sql("CREATE PROCEDURE sp_get_products() SELECT * FROM product;");
        }
    }
}
