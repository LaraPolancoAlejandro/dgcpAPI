using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dgcp.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CurrentUrlAddPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Opción 1: Eliminar y volver a crear la tabla (peligroso, se perderán los datos)
            // migrationBuilder.DropTable("CurrentUrls");
            // migrationBuilder.CreateTable( /* definición de la tabla */ );

            // Opción 2: Crear una nueva columna y copiar los datos
            migrationBuilder.AddColumn<int>("TempId", "CurrentUrls", nullable: false, defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");
            // Copiar datos (si es necesario)
            // Eliminar columna existente
            migrationBuilder.DropColumn("Id", "CurrentUrls");
            // Renombrar nueva columna
            migrationBuilder.RenameColumn("TempId", "CurrentUrls", "Id");
            // Agregar clave primaria
            migrationBuilder.AddPrimaryKey("PK_CurrentUrls", "CurrentUrls", "Id");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CurrentUrls",
                table: "CurrentUrls");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "CurrentUrls",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");
        }
    }
}
