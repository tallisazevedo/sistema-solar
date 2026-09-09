using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Configuracao;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<SolarESDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SolarES")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IConfiguracaoVersaoRepository, ConfiguracaoVersaoRepository>();
builder.Services.AddScoped<ConfiguracaoVersaoAppService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
