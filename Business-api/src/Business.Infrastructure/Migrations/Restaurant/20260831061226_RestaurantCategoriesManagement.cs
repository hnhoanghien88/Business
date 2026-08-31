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
            migrationBuilder.DropIndex(
                name: "ix_categories_parent_order",
                table: "restaurant_categories");

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_order",
                table: "restaurant_categories",
                columns: new[] { "ParentId", "DisplayOrder", "Name", "Code" });
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
            migrationBuilder.DropIndex(
                name: "ix_categories_parent_order",
                table: "restaurant_categories");

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_order",
                table: "restaurant_categories",
                columns: new[] { "ParentId", "DisplayOrder", "Name" });
        }
    }
}
