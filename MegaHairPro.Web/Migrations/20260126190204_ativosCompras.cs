using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class ativosCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustoUnitario",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "QuantidadeUnidade",
                table: "Compras");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Compras",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Compras");

            migrationBuilder.AddColumn<decimal>(
                name: "CustoUnitario",
                table: "Compras",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeUnidade",
                table: "Compras",
                type: "int",
                nullable: true);
        }
    }
}
