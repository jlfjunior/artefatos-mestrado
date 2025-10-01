using AccountService.Infrastructure;
using AccountService.Infrastructure.Options;
using AccountService.Infrastructure.Projections;
using AccountService.Infrastructure.Security;
using AccountService.Presentation;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var mongoDbOptions = builder.Configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = MongoClientSettings.FromConnectionString(mongoDbOptions.ConnectionString);
    return new MongoClient(settings);
});

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var db = client.GetDatabase(mongoDbOptions.DatabaseName);
    return db.GetCollection<UserProjection>(mongoDbOptions.CollectionName);
});

var allowedOrigin = "http://localhost:3000";


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins(allowedOrigin) // Only allow this URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/register", async (UserDto userDto, IMongoCollection<UserProjection> db) =>
{
    var passwordHasher = new PasswordHasher<UserHash>();

    var userHash = new UserHash();

    var passwordHash = passwordHasher.HashPassword(userHash, userDto.Password);

    var exists = await db.Find(f => f.Email == userDto.Email).FirstOrDefaultAsync();
    if (exists != null)
        return Results.BadRequest("User already exists");

    var accountId = Guid.NewGuid().ToString();

    var user = new UserProjection
    {
        Name = userDto.Username,
        Email = userDto.Email,
        PasswordHash = passwordHash,
        AccountId = accountId
    };

    await db.InsertOneAsync(user);

    // Create JWT token
    var tokenString = JwtBearerAuthenticationOptions.Authenticate(accountId, user.Name, user.Email);

    return Results.Ok(new UserResultDto(accountId, tokenString));
});

app.MapPost("/login", async (LoginDto login, IMongoCollection<UserProjection> db) =>
{
    var user = await db.Find(f => f.Email == login.Email).FirstOrDefaultAsync();

    if (user == null)
        return Results.Unauthorized();

    var passwordHasher = new PasswordHasher<LoginDto>();
    var result = passwordHasher.VerifyHashedPassword(login, user.PasswordHash, login.Password);

    if (result == PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    // Create JWT token
    var tokenString = JwtBearerAuthenticationOptions.Authenticate(user.AccountId, user.Name, user.Email);

    return Results.Ok(new LoginResultDto(user.AccountId, tokenString));
});


app.Run();