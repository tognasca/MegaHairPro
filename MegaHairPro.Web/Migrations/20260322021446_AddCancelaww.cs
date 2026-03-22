using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelaww : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nome",
                table: "Produtos",
                newName: "Descricao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Descricao",
                table: "Produtos",
                newName: "Nome");
        }
    }
}
