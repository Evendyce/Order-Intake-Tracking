using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OIT.Server.Application.Orders;
using OIT.Server.Infrastructure;
using OIT.Server.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDbContext<IntakeDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("OrderIntake")));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OrderService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseExceptionHandler();

// This local assignment applies versioned migrations on startup for a short setup path.
// Production deployments should run migrations as a separate controlled step.
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<IntakeDbContext>().Database.MigrateAsync();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

public partial class Program { }
