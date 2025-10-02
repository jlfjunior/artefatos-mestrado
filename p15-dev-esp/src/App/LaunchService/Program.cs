using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Application.Launch.Launch.Command.CreateLaunch;
using MassTransit;
using Quartz;
using Serilog;
using Infrastructure.Repositories.DependencyInjection;
using Infrastructure.Services.DependencyInjection;
using Application.Launch.Product.Command.CreateProduct;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile("/app/launchservice/appsettings.json", optional: false, reloadOnChange: true);

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5002);
    });

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"))
    );
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateProductCommandHandler>());
builder.Services.AddValidatorsFromAssemblyContaining<CreateLaunchCommandValidator>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7, rollOnFileSizeLimit: false, shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddLaunchRepositories();
builder.Services.AddLaunchServices();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ConsolidationCompletedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ReceiveEndpoint("daily_consolidation_response_queue", e =>
        {
            e.ConfigureConsumer<ConsolidationCompletedConsumer>(context);
        });
    });
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("DailyConsolidationJob");

    q.AddJob<DailyConsolidationJob>(opts => opts.WithIdentity(jobKey));

    var cronExpression = builder.Configuration["DailyConsolidationCronExpression"];

    if (string.IsNullOrEmpty(cronExpression))
    {
        throw new ArgumentException("DailyConsolidationCronExpression not found on config file.");
    }

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("DailyConsolidationTrigger")
        .StartNow()
        .WithCronSchedule(cronExpression)
    );
});


builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();