using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class T23OrigemSimulacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Origem",
                table: "Simulacoes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Interna");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origem",
                table: "Simulacoes");
        }
    }
}
