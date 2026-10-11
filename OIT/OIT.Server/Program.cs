using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OIT.Server.Application.Orders;
using OIT.Server.Infrastructure;
using OIT.Server.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Named statuses make the API contract readable; reject numeric values instead of accepting unknown enum IDs.

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDbContext<IntakeDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("OrderIntake")));
// Inject time so business timestamps can be controlled in tests rather than reading a static clock.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<OrderService>();
// Expose the contract for local development without introducing an additional documentation UI dependency.
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

// Authentication is outside assignment scope; this middleware alone does not restrict endpoint access.
app.UseAuthorization();

app.MapControllers();

// Published Angular routes must also work when opened directly or refreshed in the browser.
app.MapFallbackToFile("/index.html");

app.Run();

public partial class Program { }
