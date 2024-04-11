using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace INTEX_II_413.Migrations
{
    /// <inheritdoc />
    public partial class _11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserBasedRecs",
                table: "UserBasedRecs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemBasedRecs",
                table: "ItemBasedRecs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserBasedRecs");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ItemBasedRecs");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "UserBasedRecs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ItemBasedRecs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserBasedRecs",
                table: "UserBasedRecs",
                column: "ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemBasedRecs",
                table: "ItemBasedRecs",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserBasedRecs",
                table: "UserBasedRecs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemBasedRecs",
                table: "ItemBasedRecs");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "UserBasedRecs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserBasedRecs",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "ItemBasedRecs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ItemBasedRecs",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserBasedRecs",
                table: "UserBasedRecs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemBasedRecs",
                table: "ItemBasedRecs",
                column: "Id");
        }
    }
}
