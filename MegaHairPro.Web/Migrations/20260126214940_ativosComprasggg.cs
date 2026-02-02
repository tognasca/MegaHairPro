using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class ativosComprasggg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "Produtos",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Ativo",
                table: "MetodosAplicacao",
                newName: "Status");

            migrationBuilder.AddColumn<decimal>(
                name: "EstoqueGramas",
                table: "Produtos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Despesas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Venda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataVenda = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CustoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Lucro = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Margem = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venda", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Venda");

            migrationBuilder.DropColumn(
                name: "EstoqueGramas",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Despesas");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Produtos",
                newName: "Ativo");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "MetodosAplicacao",
                newName: "Ativo");
        }
    }
}
