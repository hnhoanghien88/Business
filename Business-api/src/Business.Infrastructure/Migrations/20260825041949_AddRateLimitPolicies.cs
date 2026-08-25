using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Business.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRateLimitPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rate_limit_policies",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    RoutePattern = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    HttpMethods = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    PartitionBy = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Algorithm = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    PermitLimit = table.Column<uint>(type: "int unsigned", nullable: false),
                    WindowSeconds = table.Column<uint>(type: "int unsigned", nullable: false),
                    BurstLimit = table.Column<uint>(type: "int unsigned", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Version = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rate_limit_policies", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_rate_limit_policies_Name",
                table: "rate_limit_policies",
                column: "Name",
                unique: true);
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/AddRateLimitPolicies/Up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            global::Business.Infrastructure.Persistence.Migrations.EmbeddedSql.ExecuteFolder(
                migrationBuilder,
                "Persistence/Sql/Migrations/AddRateLimitPolicies/Down");
            migrationBuilder.DropTable(
                name: "rate_limit_policies");
        }
    }
}
