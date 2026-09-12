using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class T25EnvioProposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnviosProposta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropostaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    Destino = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IdMensagemProvedor = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Tentativas = table.Column<int>(type: "integer", nullable: false),
                    UltimoErro = table.Column<string>(type: "text", nullable: true),
                    EnviadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EntregueEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnviosProposta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnviosProposta_Propostas_PropostaId",
                        column: x => x.PropostaId,
                        principalTable: "Propostas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnviosProposta_PropostaId",
                table: "EnviosProposta",
                column: "PropostaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnviosProposta");
        }
    }
}
