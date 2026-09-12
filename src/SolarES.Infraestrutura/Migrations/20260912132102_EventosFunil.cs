using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class EventosFunil : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosFunil",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SessaoFunilId = table.Column<Guid>(type: "uuid", nullable: false),
                    OcorridoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SimulacaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosFunil", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosFunil_Tipo_OcorridoEm",
                table: "EventosFunil",
                columns: new[] { "Tipo", "OcorridoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosFunil");
        }
    }
}
