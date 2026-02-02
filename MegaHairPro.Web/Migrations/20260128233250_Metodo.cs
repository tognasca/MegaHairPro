using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class Metodo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valor",
                table: "MetodosAplicacao");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "MetodosAplicacao",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "MetodosAplicacao");

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "MetodosAplicacao",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
