using CoreApiRest.IOC;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Permite deserializar Enums desde strings ("Corporativo", "Express")
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddFastExpress(); // Inyección de dependencias

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FastExpress API v1");
        c.RoutePrefix = "swagger";
    });
}

// 2. Usar la política de CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}