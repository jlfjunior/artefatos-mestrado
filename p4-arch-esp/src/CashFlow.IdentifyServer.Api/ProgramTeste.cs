//using CashFlow.IdentifyServer.Api;
//using CashFlow.IdentifyServer.Api.Database;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var cs = builder.Configuration.GetConnectionString("IdentityServerDb");
//builder.Services.AddDbContext<IdentityServerDbContext>(options => options.UseNpgsql(cs));

//builder.Services.AddDatabase(builder.Configuration);

//builder.Services
//    .AddIdentityCore<IdentityUser<Guid>>(options =>
//    {
//        options.User.RequireUniqueEmail = true;
//    })
//    .AddEntityFrameworkStores<IdentityServerDbContext>()
//    .AddApiEndpoints();

//builder.Services
//     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer();

//builder.Services.AddAuthorization();

//var app = builder.Build();

//app.UseAuthentication();
//app.UseAuthorization();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();

//    using var scope = app.Services.CreateScope();
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<IdentityServerDbContext>();
//    await context.Database.MigrateAsync();
//}

//app.UseHttpsRedirection();

//app.MapIdentityApi<IdentityUser<Guid>>();

//await app.RunAsync();

