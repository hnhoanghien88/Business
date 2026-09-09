using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business.Infrastructure.Migrations.Restaurant
{
    /// <inheritdoc />
    public partial class RestaurantCategoriesManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_order_v2",
                table: "restaurant_categories",
                columns: new[] { "ParentId", "DisplayOrder", "Name", "Code" });

            migrationBuilder.DropIndex(
                name: "ix_categories_parent_order",
                table: "restaurant_categories");

            migrationBuilder.RenameIndex(
                name: "ix_categories_parent_order_v2",
                table: "restaurant_categories",
                newName: "ix_categories_parent_order");
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/RestaurantCategoriesManagement/Up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/RestaurantCategoriesManagement/Down");
            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_order_v1",
                table: "restaurant_categories",
                columns: new[] { "ParentId", "DisplayOrder", "Name" });

            migrationBuilder.DropIndex(
                name: "ix_categories_parent_order",
                table: "restaurant_categories");

            migrationBuilder.RenameIndex(
                name: "ix_categories_parent_order_v1",
                table: "restaurant_categories",
                newName: "ix_categories_parent_order");
        }
    }
}
