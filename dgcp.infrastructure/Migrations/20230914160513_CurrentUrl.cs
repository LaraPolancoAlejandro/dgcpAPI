using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dgcp.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CurrentUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrentUrls",
                columns: table => new
                {
                    CurrentIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentUrls");
        }
    }
}
