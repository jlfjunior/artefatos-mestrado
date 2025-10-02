using FluxoCaixa.Application.Commands;
using FluxoCaixa.Infrastructure;
using FluxoCaixa.Infrastructure.Context;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===========================================================
// 🔧 Configuração de Serviços
// ===========================================================

// Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR (comandos e eventos)
builder.Services.AddMediatR(typeof(CriarLancamentoCommand).Assembly);

// Entity Framework Core com SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeção de Dependência
builder.Services.AddScoped<ILancamentoRepository, LancamentoRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ===========================================================
// 🚀 Pipeline da Aplicação
// ===========================================================

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
