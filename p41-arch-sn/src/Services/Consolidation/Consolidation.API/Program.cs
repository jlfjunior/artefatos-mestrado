using Consolidation.API.Extensions;
using Consolidation.Domain.Interfaces;
using Consolidation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Consolidation API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite seu token JWT"
    });
    c.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    };
        return requirement;
    });
});

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddHandlers();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();