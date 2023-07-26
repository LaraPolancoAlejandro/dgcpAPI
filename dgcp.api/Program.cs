using dgcp.infrastructure.Extensions;
using dgcp.domain.Abstractions;
using dgcp.api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ApiSettings>(builder.Configuration);
builder.Services.RegisterServices();
builder.Services.AddHostedService<BackgroundWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/v1/tenders", (IDataService service, int? page, int? limit) =>
{
    return service.GetTenderPagedAsync(page, limit);
});

//.WithOpenApi();

app.Run();
