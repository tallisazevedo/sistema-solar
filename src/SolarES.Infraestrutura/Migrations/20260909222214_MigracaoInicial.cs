using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolarES.Infraestrutura.Migrations
{
    /// <inheritdoc />
    public partial class MigracaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesVersao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    PublicadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicadaPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Observacao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesVersao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasExtraidas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistribuidoraId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroUc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Subgrupo = table.Column<int>(type: "integer", nullable: false),
                    TipoLigacao = table.Column<int>(type: "integer", nullable: false),
                    HistoricoConsumo = table.Column<string>(type: "jsonb", nullable: false),
                    TarifaExtraida = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    MetodoExtracao = table.Column<int>(type: "integer", nullable: false),
                    PayloadBruto = table.Column<string>(type: "jsonb", nullable: false),
                    ConfirmadaPeloClienteEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ImagemDescartadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasExtraidas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Distribuidoras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SiglaAneel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distribuidoras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Estruturas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TipoTelhado = table.Column<int>(type: "integer", nullable: false),
                    ResistenteNevoaSalina = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estruturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaixasPreco",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KwpMinimo = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    KwpMaximo = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    PrecoPorWp = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    TipoInstalacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KitLitoral = table.Column<bool>(type: "boolean", nullable: false),
                    Vigencia = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaixasPreco", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inversores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fabricante = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PotenciaW = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeMppt = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inversores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MunicipioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsentimentoLgpdEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModulosFotovoltaicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fabricante = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PotenciaW = table.Column<int>(type: "integer", nullable: false),
                    LarguraMm = table.Column<int>(type: "integer", nullable: false),
                    AlturaMm = table.Column<int>(type: "integer", nullable: false),
                    EficienciaPercentual = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ResistenteNevoaSalina = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModulosFotovoltaicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MunicipiosHsp",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoIbge = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    DistanciaMarKm = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    DistribuidoraId = table.Column<Guid>(type: "uuid", nullable: false),
                    HspPorMes = table.Column<string>(type: "jsonb", nullable: false),
                    Fonte = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MunicipiosHsp", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Propostas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SimulacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConfiguracaoVersaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidaAte = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ArquivoPdfUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EnviadaEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Canal = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propostas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Simulacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContaExtraidaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfiguracaoVersaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntradasSnapshot = table.Column<string>(type: "jsonb", nullable: false),
                    ResultadoSnapshot = table.Column<string>(type: "jsonb", nullable: false),
                    PotenciaKwp = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    QuantidadeModulos = table.Column<int>(type: "integer", nullable: false),
                    Capex = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EconomiaMensalAno1 = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaybackMeses = table.Column<int>(type: "integer", nullable: true),
                    Tir = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Vpl = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CoberturaPercentual = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    RoteadaParaHumano = table.Column<bool>(type: "boolean", nullable: false),
                    MotivoRoteamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Simulacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TarifasVigentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistribuidoraId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subgrupo = table.Column<int>(type: "integer", nullable: false),
                    TarifaTe = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    TarifaTusd = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ValorFioBPorKwh = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    AliquotaIcms = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    AliquotaPisCofins = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    VigenciaInicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VigenciaFim = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolucaoHomologatoria = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fonte = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasVigentes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SenhaHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Perfil = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesVersao_Status",
                table: "ConfiguracoesVersao",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CriadoEm",
                table: "Leads",
                column: "CriadoEm",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Status",
                table: "Leads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MunicipiosHsp_CodigoIbge",
                table: "MunicipiosHsp",
                column: "CodigoIbge",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_Numero",
                table: "Propostas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_ValidaAte",
                table: "Propostas",
                column: "ValidaAte");

            migrationBuilder.CreateIndex(
                name: "IX_Simulacoes_ConfiguracaoVersaoId",
                table: "Simulacoes",
                column: "ConfiguracaoVersaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Simulacoes_LeadId",
                table: "Simulacoes",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasVigentes_DistribuidoraId_Subgrupo_VigenciaInicio",
                table: "TarifasVigentes",
                columns: new[] { "DistribuidoraId", "Subgrupo", "VigenciaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesVersao");

            migrationBuilder.DropTable(
                name: "ContasExtraidas");

            migrationBuilder.DropTable(
                name: "Distribuidoras");

            migrationBuilder.DropTable(
                name: "Estruturas");

            migrationBuilder.DropTable(
                name: "FaixasPreco");

            migrationBuilder.DropTable(
                name: "Inversores");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "ModulosFotovoltaicos");

            migrationBuilder.DropTable(
                name: "MunicipiosHsp");

            migrationBuilder.DropTable(
                name: "Propostas");

            migrationBuilder.DropTable(
                name: "Simulacoes");

            migrationBuilder.DropTable(
                name: "TarifasVigentes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
