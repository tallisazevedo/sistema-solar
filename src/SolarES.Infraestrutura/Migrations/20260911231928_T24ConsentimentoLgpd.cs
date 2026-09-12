using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class T24ConsentimentoLgpd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsentimentosLgpd",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Finalidade = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    VersaoTexto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConcedidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentimentosLgpd", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentimentosLgpd_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentimentosLgpd_LeadId_Finalidade",
                table: "ConsentimentosLgpd",
                columns: new[] { "LeadId", "Finalidade" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsentimentosLgpd");
        }
    }
}
