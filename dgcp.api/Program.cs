using dgcp.infrastructure.Extensions;
using dgcp.domain.Abstractions;
using dgcp.api;
using dgcp.infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using dgcp.infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<AppDbContext>(); // Reemplaza con tu DbContext
builder.Services.AddControllers(); // ¡No olvides esta línea!
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUsersService, UsersService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ApiSettings>(builder.Configuration);
builder.Services.RegisterServices();
builder.Services.AddHostedService<BackgroundWorker>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtIssuer"], // Asegúrate de tener esta configuración en tu archivo appsettings.json
        ValidAudience = builder.Configuration["JwtIssuer"], // Asegúrate de tener esta configuración en tu archivo appsettings.json
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtKey"])) // Asegúrate de tener esta configuración en tu archivo appsettings.json
    };
});
var app = builder.Build();
app.UseAuthentication(); // Asegúrate de que esta línea esté antes de app.UseAuthorization() y app.MapControllers()
app.UseAuthorization();
app.MapControllers();
app.UseCors();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/v1/tenders", (IDataService service, int? page, int? limit, DateTime? startDate, DateTime? endDate, string? empresa, string rubros) =>
{
    return service.GetTenderPagedAsync(page, limit, startDate, endDate, empresa, rubros);
});

//.WithOpenApi();

app.Run();
