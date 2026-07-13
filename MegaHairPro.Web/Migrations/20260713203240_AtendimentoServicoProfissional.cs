using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AtendimentoServicoProfissional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfissionalId",
                table: "AgendamentoServicos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Realizado",
                table: "AgendamentoServicos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConfiguracoesBooking",
                columns: table => new
                {
                    Chave = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesBooking", x => x.Chave);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosClientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosClientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendamentoServicos_ProfissionalId",
                table: "AgendamentoServicos",
                column: "ProfissionalId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosClientes_ClienteId",
                table: "UsuariosClientes",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgendamentoServicos_Profissionais_ProfissionalId",
                table: "AgendamentoServicos",
                column: "ProfissionalId",
                principalTable: "Profissionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgendamentoServicos_Profissionais_ProfissionalId",
                table: "AgendamentoServicos");

            migrationBuilder.DropTable(
                name: "ConfiguracoesBooking");

            migrationBuilder.DropTable(
                name: "UsuariosClientes");

            migrationBuilder.DropIndex(
                name: "IX_AgendamentoServicos_ProfissionalId",
                table: "AgendamentoServicos");

            migrationBuilder.DropColumn(
                name: "ProfissionalId",
                table: "AgendamentoServicos");

            migrationBuilder.DropColumn(
                name: "Realizado",
                table: "AgendamentoServicos");
        }
    }
}
