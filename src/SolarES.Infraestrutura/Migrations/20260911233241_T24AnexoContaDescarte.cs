using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class T24AnexoContaDescarte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DescartadoEm",
                table: "AnexosConta",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DescartarAte",
                table: "AnexosConta",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_AnexosConta_DescartarAte",
                table: "AnexosConta",
                column: "DescartarAte");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnexosConta_DescartarAte",
                table: "AnexosConta");

            migrationBuilder.DropColumn(
                name: "DescartadoEm",
                table: "AnexosConta");

            migrationBuilder.DropColumn(
                name: "DescartarAte",
                table: "AnexosConta");
        }
    }
}
