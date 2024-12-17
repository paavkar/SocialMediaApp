using SocialMediaApp.Server.CosmosDb;
using SocialMediaApp.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<CosmosDbFactory>();
builder.Services.AddScoped<ICosmosDbUserService, CosmosDbUserService>();
builder.Services.AddScoped<RoleManager>();
builder.Services.AddScoped<UserManager>();
builder.Services.AddScoped<SignInManager>();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
