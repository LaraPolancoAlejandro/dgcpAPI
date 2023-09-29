using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dgcp.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenderItems_Tenders_TenderReleaseOcid",
                table: "TenderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenderItems",
                table: "TenderItems");

            migrationBuilder.RenameTable(
                name: "TenderItems",
                newName: "TenderItem");

            migrationBuilder.RenameIndex(
                name: "IX_TenderItems_TenderReleaseOcid",
                table: "TenderItem",
                newName: "IX_TenderItem_TenderReleaseOcid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenderItem",
                table: "TenderItem",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserNameOrEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsProjectAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TenderItem_Tenders_TenderReleaseOcid",
                table: "TenderItem",
                column: "TenderReleaseOcid",
                principalTable: "Tenders",
                principalColumn: "ReleaseOcid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenderItem_Tenders_TenderReleaseOcid",
                table: "TenderItem");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TenderItem",
                table: "TenderItem");

            migrationBuilder.RenameTable(
                name: "TenderItem",
                newName: "TenderItems");

            migrationBuilder.RenameIndex(
                name: "IX_TenderItem_TenderReleaseOcid",
                table: "TenderItems",
                newName: "IX_TenderItems_TenderReleaseOcid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenderItems",
                table: "TenderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TenderItems_Tenders_TenderReleaseOcid",
                table: "TenderItems",
                column: "TenderReleaseOcid",
                principalTable: "Tenders",
                principalColumn: "ReleaseOcid");
        }
    }
}
