using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MegaHairPro.Web.Migrations
{
    /// <inheritdoc />
    public partial class CriarUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Perfil",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenhaHash",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Perfil",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SenhaHash",
                table: "Usuarios");
        }
    }
}
