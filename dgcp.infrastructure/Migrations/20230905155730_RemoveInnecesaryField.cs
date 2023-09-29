using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dgcp.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInnecesaryField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicationPolicy",
                table: "TendersFinal");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "TendersFinal");

            migrationBuilder.DropColumn(
                name: "PublicationPolicy",
                table: "Tenders");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "Tenders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicationPolicy",
                table: "TendersFinal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                table: "TendersFinal",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicationPolicy",
                table: "Tenders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedDate",
                table: "Tenders",
                type: "datetime2",
                nullable: true);
        }
    }
}
