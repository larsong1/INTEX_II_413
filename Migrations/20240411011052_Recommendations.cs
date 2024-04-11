using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INTEX_II_413.Migrations
{
    /// <inheritdoc />
    public partial class Recommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemBasedRecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Recommendation1 = table.Column<int>(type: "int", nullable: false),
                    Recommendation2 = table.Column<int>(type: "int", nullable: false),
                    Recommendation3 = table.Column<int>(type: "int", nullable: false),
                    Recommendation4 = table.Column<int>(type: "int", nullable: false),
                    Recommendation5 = table.Column<int>(type: "int", nullable: false),
                    Recommendation6 = table.Column<int>(type: "int", nullable: false),
                    Recommendation7 = table.Column<int>(type: "int", nullable: false),
                    Recommendation8 = table.Column<int>(type: "int", nullable: false),
                    Recommendation9 = table.Column<int>(type: "int", nullable: false),
                    Recommendation10 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBasedRecs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBasedRecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Recommendation1 = table.Column<int>(type: "int", nullable: false),
                    Recommendation2 = table.Column<int>(type: "int", nullable: false),
                    Recommendation3 = table.Column<int>(type: "int", nullable: false),
                    Recommendation4 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBasedRecs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemBasedRecs");

            migrationBuilder.DropTable(
                name: "UserBasedRecs");
        }
    }
}
