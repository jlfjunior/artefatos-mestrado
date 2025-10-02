using Boxflux.Application.Handlers.Lauchings;
using Boxflux.Domain.Interfaces;
using Boxflux.Infra.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração do MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(ConsolidatedDailyCommandHandler).Assembly,
        typeof(LauchingCommandHandler).Assembly);
});

// Configuração do DbContext
builder.Services.AddDbContext<BoxfluxContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 31))
    )
);

//Repositories


builder.Services.AddScoped<IGeralRepository<ConsolidatedDaily>, ConsolidatedDailyRepository>();
builder.Services.AddScoped<IGeralRepository<Lauching>, LauchingRepository>();

// Configuração de outros serviços
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BoxFlux.API", Version = "v1" });
    //c.OperationFilter<SwaggerFileOperationFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BoxFlux.API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseHttpsRedirection();
app.MapControllers();


app.Run();
