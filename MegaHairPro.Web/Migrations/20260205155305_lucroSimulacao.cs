using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class lucroSimulacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MargemPercentual",
                table: "Simulacao",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentualDesconto",
                table: "Simulacao",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoVendaOriginal",
                table: "Simulacao",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorDesconto",
                table: "Simulacao",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MargemPercentual",
                table: "Simulacao");

            migrationBuilder.DropColumn(
                name: "PercentualDesconto",
                table: "Simulacao");

            migrationBuilder.DropColumn(
                name: "PrecoVendaOriginal",
                table: "Simulacao");

            migrationBuilder.DropColumn(
                name: "ValorDesconto",
                table: "Simulacao");
        }
    }
}
