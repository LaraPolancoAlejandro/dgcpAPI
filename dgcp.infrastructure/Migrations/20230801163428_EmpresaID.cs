using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dgcp.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmpresaID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenderItem_Tenders_TenderReleaseOcid",
                table: "TenderItem");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TendersFinal");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Tenders");

            migrationBuilder.RenameColumn(
                name: "Empresa",
                table: "TendersFinal",
                newName: "EmpresaIds");

            migrationBuilder.RenameColumn(
                name: "Empresa",
                table: "Tenders",
                newName: "EmpresaIds");

            migrationBuilder.AlterColumn<string>(
                name: "TenderReleaseOcid",
                table: "TenderItem",
                type: "nvarchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

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

            migrationBuilder.RenameColumn(
                name: "EmpresaIds",
                table: "TendersFinal",
                newName: "Empresa");

            migrationBuilder.RenameColumn(
                name: "EmpresaIds",
                table: "Tenders",
                newName: "Empresa");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TendersFinal",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Tenders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "TenderReleaseOcid",
                table: "TenderItem",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TenderItem_Tenders_TenderReleaseOcid",
                table: "TenderItem",
                column: "TenderReleaseOcid",
                principalTable: "Tenders",
                principalColumn: "ReleaseOcid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
