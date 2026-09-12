using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class VencimentoPropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ResponsavelUsuarioId",
                table: "Propostas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VencidaEm",
                table: "Propostas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VencimentoNotificadoEm",
                table: "Propostas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_ResponsavelUsuarioId",
                table: "Propostas",
                column: "ResponsavelUsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Propostas_Usuarios_ResponsavelUsuarioId",
                table: "Propostas",
                column: "ResponsavelUsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Propostas_Usuarios_ResponsavelUsuarioId",
                table: "Propostas");

            migrationBuilder.DropIndex(
                name: "IX_Propostas_ResponsavelUsuarioId",
                table: "Propostas");

            migrationBuilder.DropColumn(
                name: "ResponsavelUsuarioId",
                table: "Propostas");

            migrationBuilder.DropColumn(
                name: "VencidaEm",
                table: "Propostas");

            migrationBuilder.DropColumn(
                name: "VencimentoNotificadoEm",
                table: "Propostas");
        }
    }
}
