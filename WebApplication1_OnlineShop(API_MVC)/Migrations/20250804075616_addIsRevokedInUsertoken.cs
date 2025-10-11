using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1_API_MVC_.Migrations
{
    /// <inheritdoc />
    public partial class addIsRevokedInUsertoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "Tokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "Tokens");
        }
    }
}
