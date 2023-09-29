using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dgcp.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CurrentIndexVisitedUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "VisitedUrls");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "VisitedUrls");

            migrationBuilder.AddColumn<int>(
                name: "CurrentIndex",
                table: "VisitedUrls",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentIndex",
                table: "VisitedUrls");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "VisitedUrls",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "VisitedUrls",
                type: "datetime2",
                nullable: true);
        }
    }
}
