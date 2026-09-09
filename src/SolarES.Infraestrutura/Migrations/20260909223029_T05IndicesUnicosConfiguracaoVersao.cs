using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class T05IndicesUnicosConfiguracaoVersao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesVersao_UmaPublicada",
                table: "ConfiguracoesVersao",
                column: "Status",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesVersao_UmRascunho",
                table: "ConfiguracoesVersao",
                column: "Status",
                unique: true,
                filter: "\"Status\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConfiguracoesVersao_UmaPublicada",
                table: "ConfiguracoesVersao");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracoesVersao_UmRascunho",
                table: "ConfiguracoesVersao");
        }
    }
}
