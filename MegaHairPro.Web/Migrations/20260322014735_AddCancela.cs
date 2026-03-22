using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCancela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgendamentoId",
                table: "ContasFinanceiras",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConclusao",
                table: "Agendamentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamentoSinal",
                table: "Agendamentos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentualSinal",
                table: "Agendamentos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RestantePago",
                table: "Agendamentos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SinalPago",
                table: "Agendamentos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Agendamentos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorRestante",
                table: "Agendamentos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorSinal",
                table: "Agendamentos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ContasFinanceiras_AgendamentoId",
                table: "ContasFinanceiras",
                column: "AgendamentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContasFinanceiras_Agendamentos_AgendamentoId",
                table: "ContasFinanceiras",
                column: "AgendamentoId",
                principalTable: "Agendamentos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContasFinanceiras_Agendamentos_AgendamentoId",
                table: "ContasFinanceiras");

            migrationBuilder.DropIndex(
                name: "IX_ContasFinanceiras_AgendamentoId",
                table: "ContasFinanceiras");

            migrationBuilder.DropColumn(
                name: "AgendamentoId",
                table: "ContasFinanceiras");

            migrationBuilder.DropColumn(
                name: "DataConclusao",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "DataPagamentoSinal",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "PercentualSinal",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "RestantePago",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "SinalPago",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "ValorRestante",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "ValorSinal",
                table: "Agendamentos");
        }
    }
}
