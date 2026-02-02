using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lucro",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "Margem",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "Venda");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Venda",
                newName: "ProdutoNome");

            migrationBuilder.AlterColumn<decimal>(
                name: "CustoTotal",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Venda",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ClienteNome",
                table: "Venda",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LucroTotal",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoVenda",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorComissaoCabeleireira",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorComissaoVendedora",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "ClienteNome",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "LucroTotal",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "PrecoVenda",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "ValorComissaoCabeleireira",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "ValorComissaoVendedora",
                table: "Venda");

            migrationBuilder.RenameColumn(
                name: "ProdutoNome",
                table: "Venda",
                newName: "Status");

            migrationBuilder.AlterColumn<decimal>(
                name: "CustoTotal",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "Lucro",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Margem",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "Venda",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
