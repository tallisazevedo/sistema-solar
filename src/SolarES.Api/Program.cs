using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SolarES.Api;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Identidade;
using SolarES.Aplicacao.Propostas;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Infraestrutura.Identidade;
using SolarES.Infraestrutura.Pdf;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options => options.AddPolicy("Landing", policy =>
{
    var origens = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];
    if (origens.Length > 0)
    {
        policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod();
    }
}));

builder.Services.AddDbContext<SolarESDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SolarES")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IConfiguracaoVersaoRepository, ConfiguracaoVersaoRepository>();
builder.Services.AddScoped<ConfiguracaoVersaoAppService>();
builder.Services.AddScoped(typeof(IRepositorioCrud<>), typeof(EfRepositorioCrud<>));

builder.Services.AddScoped<IUsuarioRepository, EfUsuarioRepository>();
builder.Services.AddScoped<IGeradorTokenJwt, GeradorTokenJwt>();
builder.Services.AddScoped<AutenticacaoAppService>();

builder.Services.AddScoped<ISimulacaoRepository, EfSimulacaoRepository>();
builder.Services.AddScoped<SimulacaoAppService>();

builder.Services.AddScoped<IPropostaRepository, EfPropostaRepository>();
builder.Services.AddScoped<IGeradorPdfProposta, GeradorPdfProposta>();
builder.Services.AddScoped<IArmazenamentoPdf, ArmazenamentoPdfEmDisco>();
builder.Services.AddScoped<GerarPdfPropostaJob>();
builder.Services.AddScoped<PropostaAppService>();

builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("SolarES")),
        new PostgreSqlStorageOptions { SchemaName = "hangfire" }));
builder.Services.AddHangfireServer();

var segredoJwt = builder.Configuration["Jwt:Segredo"]
    ?? throw new InvalidOperationException("Configuracao 'Jwt:Segredo' ausente.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Emissor"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(segredoJwt)),
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddExceptionHandler<ExcecaoDeValidacaoDominioHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// AllowAnonymous: sem isso, o FallbackPolicy (RequireAuthenticatedUser, pensado pra
// API JWT) intercepta a requisicao do dashboard antes do
// AcessoLocalHangfireDashboardFilter rodar -- o controle de acesso daqui e' o filtro
// de loopback acima, nao o bearer da Api.
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new AcessoLocalHangfireDashboardFilter()],
}).AllowAnonymous();

app.Run();

public partial class Program;
