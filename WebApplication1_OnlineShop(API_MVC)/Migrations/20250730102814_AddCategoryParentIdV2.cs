using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1_API_MVC_.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryParentIdV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryParentId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryParentId",
                table: "Categories",
                column: "CategoryParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_CategoryParentId",
                table: "Categories",
                column: "CategoryParentId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_CategoryParentId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CategoryParentId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CategoryParentId",
                table: "Categories");
        }
    }
}
