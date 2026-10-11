using Microsoft.EntityFrameworkCore;
using OIT.Server.Application.Orders;
using OIT.Server.Contracts.Clients;
using OIT.Server.Contracts.Orders;
using OIT.Server.Controllers.Clients;
using OIT.Server.Domain;
using OIT.Server.Domain.Orders;
using OIT.Server.Persistence;

namespace OIT.Tests.Orders;

// Real SQL Server tests exercise uniqueness and transactions that mocks cannot prove.
public class OrderPersistenceTests : IAsyncLifetime
{
    private readonly string connection = $"Server=(localdb)\\OITLocalDB;Database=OIT_Tests_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True";
    private IntakeDbContext Context() => new(new DbContextOptionsBuilder<IntakeDbContext>().UseSqlServer(connection).Options);
    public async Task InitializeAsync() { await using var db = Context(); await db.Database.MigrateAsync(); }
    public async Task DisposeAsync() { await using var db = Context(); await db.Database.EnsureDeletedAsync(); }
    private static OrderInput Input() => new()
    {
        ExternalReference = "REF-1", NewClient = new ClientInput("Jane", "jane@example.com"),
        Currency = "USD", Items = [new("ABC", "Widget", 2, 10.25m)]
    };

    [Fact, Trait("Category", "Integration")]
    public async Task RepeatSubmissionReturnsOverviewAndConflictingContentDoesNotOverwrite()
    {
        await using var db = Context(); var service = new OrderService(db, TimeProvider.System);
        var first = await service.Create(Input(), default);
        var repeated = await service.Create(Input(), default);
        Assert.False(first.IsDuplicate); Assert.True(repeated.IsDuplicate);
        Assert.Equal(first.Overview.Id, repeated.Overview.Id); Assert.Empty(repeated.Differences);
        var changed = Input(); changed.Items[0] = new("ABC", "Widget", 3, 10.25m);
        var conflict = await service.Create(changed, default);
        Assert.Equal(["Line items"], conflict.Differences);
        Assert.Equal(20.50m, conflict.Overview.Total);
        Assert.Equal(1, await db.Orders.CountAsync()); Assert.Equal(1, await db.Clients.CountAsync());
    }

    [Fact, Trait("Category", "Integration")]
    public async Task SimultaneousSubmissionCreatesExactlyOneClientAndOrder()
    {
        async Task<SubmissionView> Submit()
        {
            await using var db = Context();
            return await new OrderService(db, TimeProvider.System).Create(Input(), default);
        }
        var results = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Submit()));
        Assert.Single(results, r => !r.IsDuplicate);
        Assert.Single(results.Select(r => r.Overview.Id).Distinct());
        await using var verify = Context();
        Assert.Equal(1, await verify.Clients.CountAsync()); Assert.Equal(1, await verify.Orders.CountAsync());
    }

    [Fact, Trait("Category", "Integration")]
    public async Task DuplicateWithDifferentNewClientDoesNotCreateOrphanClient()
    {
        await using var db = Context(); var service = new OrderService(db, TimeProvider.System);
        await service.Create(Input(), default);
        var input = Input(); input.NewClient = new("Other", "other@example.com");
        var duplicate = await service.Create(input, default);
        Assert.Contains("Client", duplicate.Differences);
        Assert.Equal(1, await db.Clients.CountAsync());
    }

    [Fact, Trait("Category", "Integration")]
    public async Task PendingEditReplacesItemsAndStaleVersionIsRejected()
    {
        await using var db = Context(); var service = new OrderService(db, TimeProvider.System);
        var first = await service.Create(Input(), default);
        var input = Input(); input.ClientId = first.Overview.ClientId; input.NewClient = null;
        input.Version = first.Overview.Version; input.Items = [new("NEW", "Replacement", 3, 0.10m)];
        var updated = await service.Update(first.Overview.Id, input, default);
        Assert.Equal(0.30m, updated.Total);
        await using var verify = Context();
        Assert.Equal(1, await verify.OrderLines.CountAsync());
        Assert.Equal(409, (await Assert.ThrowsAsync<BusinessException>(() =>
            service.Status(first.Overview.Id, new(OrderStatus.Confirmed, first.Overview.Version), default))).Status);
    }

    [Fact, Trait("Category", "Integration")]
    public async Task RemovingReferencedClientPreservesHistoryAndPreventsNewOrders()
    {
        await using var db = Context(); var service = new OrderService(db, TimeProvider.System);
        var first = await service.Create(Input(), default);
        var client = await db.Clients.SingleAsync();
        await new ClientsController(db).Remove(client.Id, client.Version, default);
        Assert.False(client.IsActive); Assert.Equal(1, await db.Orders.CountAsync());
        var next = Input(); next.ExternalReference = "REF-2";
        Assert.Equal(409, (await Assert.ThrowsAsync<BusinessException>(() => service.Create(next, default))).Status);
        Assert.True((await service.Create(Input(), default)).IsDuplicate);
    }

    [Fact, Trait("Category", "Integration")]
    public async Task ClientEditsDoNotRewriteOrderSnapshots()
    {
        await using var db = Context(); var service = new OrderService(db, TimeProvider.System);
        var first = await service.Create(Input(), default);
        var client = await db.Clients.SingleAsync();
        await new ClientsController(db).Update(client.Id,
            new("Updated", "updated@example.com", true, client.Version), default);
        await using var verify = Context();
        var saved = await verify.Orders.SingleAsync();
        Assert.Equal("Jane", saved.ClientName); Assert.Equal("jane@example.com", saved.ClientEmail);
    }
}
