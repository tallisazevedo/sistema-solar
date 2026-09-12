using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class StatusLeadCadastroManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "MunicipioId",
                table: "Leads",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "VisitaTecnicaAgendadaPara",
                table: "Leads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistoricosStatusLead",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusAnterior = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StatusNovo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlteradoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosStatusLead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricosStatusLead_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricosStatusLead_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosStatusLead_LeadId_AlteradoEm",
                table: "HistoricosStatusLead",
                columns: new[] { "LeadId", "AlteradoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosStatusLead_UsuarioId",
                table: "HistoricosStatusLead",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricosStatusLead");

            migrationBuilder.DropColumn(
                name: "VisitaTecnicaAgendadaPara",
                table: "Leads");

            migrationBuilder.AlterColumn<Guid>(
                name: "MunicipioId",
                table: "Leads",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
