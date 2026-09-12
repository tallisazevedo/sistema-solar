using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class T26AceiteEPerdaProposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AceitaEm",
                table: "Propostas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoPerda",
                table: "Propostas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PerdidaEm",
                table: "Propostas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AceitaEm",
                table: "Propostas");

            migrationBuilder.DropColumn(
                name: "MotivoPerda",
                table: "Propostas");

            migrationBuilder.DropColumn(
                name: "PerdidaEm",
                table: "Propostas");
        }
    }
}
