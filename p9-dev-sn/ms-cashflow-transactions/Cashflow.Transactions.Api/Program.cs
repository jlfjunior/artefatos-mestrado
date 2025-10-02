using Cashflow.Transactions.Api.Bootstrap;
using Cashflow.Transactions.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerConfiguration.Configure);

builder.Services.ConfigureApp(builder.Configuration);
builder.Services.ConfigureAuth(builder.Configuration);
builder.Services.ConfigureData(builder.Configuration);
builder.Services.ConfigureEvent(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cashflow Transactions Api Documentation"));
    app.UseCors("DevelopmentCors");
}

app.UseCors("ProductionCors");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/api").MapTransactionsEndpoints();

app.Run();
