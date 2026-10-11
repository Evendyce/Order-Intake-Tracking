using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OIT.Server.Contracts.Clients;
using OIT.Server.Contracts.Orders;
using OIT.Server.Persistence;

namespace OIT.Tests.Api;

public class OrderApiTests : IAsyncLifetime
{
    private readonly string connection = $"Server=(localdb)\\OITDevelopment;Database=OIT_ApiTests_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True";
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient http = null!;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    { Converters = { new JsonStringEnumConverter() } };
    public Task InitializeAsync()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ConnectionStrings:OrderIntake"] = connection })));
        http = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        // Refuse to send write requests if startup ever bypasses the test connection override.
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IntakeDbContext>().Database.GetDbConnection().Database;
        Assert.Equal(new SqlConnectionStringBuilder(connection).InitialCatalog, database);
        return Task.CompletedTask;
    }
    public async Task DisposeAsync()
    {
        http.Dispose(); await factory.DisposeAsync();
        await using var db = new IntakeDbContext(new DbContextOptionsBuilder<IntakeDbContext>().UseSqlServer(connection).Options);
        await db.Database.EnsureDeletedAsync();
    }
    private static OrderInput Input() => new()
    {
        ExternalReference = "API-1", NewClient = new ClientInput("Demo", "demo@example.com"),
        Items = [new("SKU", "Item", 3, 19.99m)]
    };

    [Fact, Trait("Category", "Integration")]
    public async Task PostReturnsLocationAndDuplicateOverviewWithAppropriateStatus()
    {
        var created = await http.PostAsJsonAsync("/api/orders", Input());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var body = await created.Content.ReadFromJsonAsync<SubmissionView>(Json);
        Assert.Equal(59.97m, body!.Overview.Total);
        var repeated = await http.PostAsJsonAsync("/api/orders", Input());
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        var input = Input(); input.Notes = "Changed";
        var conflict = await http.PostAsJsonAsync("/api/orders", input);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.True((await conflict.Content.ReadFromJsonAsync<SubmissionView>(Json))!.IsDuplicate);
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync(created.Headers.Location)).StatusCode);
    }

    [Theory, Trait("Category", "Integration")]
    [InlineData("0", "1")]
    [InlineData("1.5", "1")]
    [InlineData("1", "-1")]
    public async Task InvalidLineNumbersReturnValidationProblem(string quantity, string price)
    {
        var json = $$"""{"externalReference":"BAD","newClient":{"name":"Demo","email":"demo@example.com"},"currency":"USD","items":[{"sku":"SKU","name":"Item","quantity":{{quantity}},"unitPrice":{{price}}}]}""";
        var response = await http.PostAsync("/api/orders", new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("errors", await response.Content.ReadAsStringAsync());
    }

    [Fact, Trait("Category", "Integration")]
    public async Task FiltersAndStatusRulesWorkThroughHttp()
    {
        var created = await http.PostAsJsonAsync("/api/orders", Input());
        var order = (await created.Content.ReadFromJsonAsync<SubmissionView>(Json))!.Overview;
        var bad = await http.PatchAsJsonAsync($"/api/orders/{order.Id}/status", new { status = "Fulfilled", order.Version });
        Assert.Equal(HttpStatusCode.Conflict, bad.StatusCode);
        var confirmed = await http.PatchAsJsonAsync($"/api/orders/{order.Id}/status", new { status = "Confirmed", order.Version });
        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        var filtered = await http.GetFromJsonAsync<OrderView[]>("/api/orders?reference=api&status=Confirmed", Json);
        Assert.Single(filtered!);
        Assert.Empty((await http.GetFromJsonAsync<OrderView[]>("/api/orders?status=Pending", Json))!);
    }

    [Fact, Trait("Category", "Integration")]
    public async Task ClientCrudValidatesEmailsAndRemovesUnreferencedClient()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await http.PostAsJsonAsync("/api/clients", new { name = "Demo", email = "invalid" })).StatusCode);
        var created = await http.PostAsJsonAsync("/api/clients", new { name = "Demo", email = "demo@example.com" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var client = (await created.Content.ReadFromJsonAsync<ClientView>())!;
        var updated = await http.PutAsJsonAsync($"/api/clients/{client.Id}", new { name = "Changed", client.Email, isActive = true, client.Version });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        client = (await updated.Content.ReadFromJsonAsync<ClientView>())!;
        Assert.Equal(HttpStatusCode.NoContent, (await http.DeleteAsync($"/api/clients/{client.Id}?version={client.Version}")).StatusCode);
        Assert.Empty((await http.GetFromJsonAsync<ClientView[]>("/api/clients"))!);
    }
}
