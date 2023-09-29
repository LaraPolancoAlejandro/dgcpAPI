using dgcp.infrastructure.Extensions;
using dgcp.domain.Abstractions;
using dgcp.api;
using Microsoft.AspNetCore.Mvc;
using dgcp.domain.Models;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

var app = builder.Build();
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
