using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class ProdutoCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Simulacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    QuantidadeGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstoqueAtualGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstoquePosVendaGramas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustoCabelo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PercentualLucroCabelo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorLucroCabelo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaoDeObra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tinta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CorteEscova = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DespesaSalaoRateada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentualComissaoVendedora = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorComissaoVendedora = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PercentualComissaoCabeleireira = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorComissaoCabeleireira = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PrecoVenda = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LucroLiquido = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulacao", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Simulacao");
        }
    }
}
