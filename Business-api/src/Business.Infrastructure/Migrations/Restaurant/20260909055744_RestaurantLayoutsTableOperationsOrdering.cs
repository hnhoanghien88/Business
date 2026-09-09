using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business.Infrastructure.Migrations.Restaurant
{
    /// <inheritdoc />
    public partial class RestaurantLayoutsTableOperationsOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientRequestId",
                table: "restaurant_orders",
                type: "char(36)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE restaurant_orders SET ClientRequestId = UUID() WHERE ClientRequestId IS NULL");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientRequestId",
                table: "restaurant_orders",
                type: "char(36)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "uk_orders_client_request_id",
                table: "restaurant_orders",
                column: "ClientRequestId",
                unique: true);
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/RestaurantLayoutsTableOperationsOrdering/Up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/RestaurantLayoutsTableOperationsOrdering/Down");
            migrationBuilder.DropIndex(
                name: "uk_orders_client_request_id",
                table: "restaurant_orders");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "restaurant_orders");
        }
    }
}
