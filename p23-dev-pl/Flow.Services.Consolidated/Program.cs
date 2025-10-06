using Flow.Application;
using Flow.Services.Consolidated.Consumers;
using Flow.Infra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<KafkaBackgroundConsumer>();

builder.Services.AddDependencesConsolidatedInfra(builder.Configuration);
builder.Services.AddDependencyInjectionConsolidatedApplication(builder.Configuration);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
