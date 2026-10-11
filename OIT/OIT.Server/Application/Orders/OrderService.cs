using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OIT.Server.Contracts.Orders;
using OIT.Server.Domain;
using OIT.Server.Domain.Clients;
using OIT.Server.Domain.Orders;
using OIT.Server.Persistence;

namespace OIT.Server.Application.Orders;

/// <summary>
/// Coordinates order persistence while leaving calculations and lifecycle policy in OrderRules.
/// A scoped context keeps each request's transaction and tracked entities together.
/// </summary>
public class OrderService(IntakeDbContext db, TimeProvider clock)
{
    public async Task<SubmissionView> Create(OrderInput input, CancellationToken ct)
    {
        var candidate = new Order { ExternalReference = input.ExternalReference.Trim(),
            NormalizedReference = OrderRules.Normalize(input.ExternalReference) };
        OrderRules.ApplyDetails(candidate, input);
        if (input.ClientId.HasValue == (input.NewClient is not null))
            throw new BusinessException(400, "Select an existing client or provide a new client, not both.");

        // Retry unique-key races after SaveChanges rolls back the entire client/order batch.
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var client = input.ClientId.HasValue
                ? await db.Clients.FindAsync([input.ClientId.Value], ct)
                : await db.Clients.SingleOrDefaultAsync(c => c.NormalizedEmail == OrderRules.Normalize(input.NewClient!.Email), ct);
            if (input.ClientId.HasValue && client is null) throw new BusinessException(404, "Client not found.");
            var email = client?.Email ?? input.NewClient!.Email.Trim();
            var existing = await db.Orders.Include(o => o.Items)
                .SingleOrDefaultAsync(o => o.NormalizedReference == candidate.NormalizedReference, ct);
            // Resolve duplicates before inserting a client or enforcing new-order availability.
            // A repeat submission reviews saved history even if that client is now inactive.
            if (existing is not null)
                return new(true, OrderRules.View(existing), OrderRules.Differences(existing, input, email));
            if (client is { IsActive: false }) throw new BusinessException(409, "Reactivate this client before creating an order.");
            client ??= new Client { Name = input.NewClient!.Name.Trim(), Email = email,
                NormalizedEmail = OrderRules.Normalize(email) };
            candidate.Client = client;
            candidate.ClientId = client.Id;
            candidate.ClientName = client.Name;
            candidate.ClientEmail = client.Email;
            candidate.CreatedAt = candidate.UpdatedAt = clock.GetUtcNow();
            db.Orders.Add(candidate);
            try
            {
                // One SaveChanges transaction prevents an orphan client if order insertion fails.
                await db.SaveChangesAsync(ct);
                return new(false, OrderRules.View(candidate), []);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                db.ChangeTracker.Clear();
            }
        }
        throw new BusinessException(409, "Another submission changed this reference or client. Please retry.");
    }

    public async Task<Order> Find(Guid id, CancellationToken ct) =>
        await db.Orders.Include(o => o.Items).SingleOrDefaultAsync(o => o.Id == id, ct)
        ?? throw new BusinessException(404, "Order not found.");

    public async Task<OrderView> Update(Guid id, OrderInput input, CancellationToken ct)
    {
        var order = await Find(id, ct);
        CheckVersion(order, input.Version);
        if (OrderRules.Normalize(input.ExternalReference) != order.NormalizedReference)
            throw new BusinessException(409, "The external reference cannot be changed.");
        if (!input.ClientId.HasValue || input.NewClient is not null)
            throw new BusinessException(400, "Select an existing client when modifying an order.");
        var client = await db.Clients.FindAsync([input.ClientId.Value], ct)
            ?? throw new BusinessException(404, "Client not found.");
        if (!client.IsActive && client.Id != order.ClientId)
            throw new BusinessException(409, "An inactive client cannot be assigned to an order.");
        var previousItems = order.Items.ToArray();
        OrderRules.ApplyDetails(order, input);
        db.OrderLines.RemoveRange(previousItems);
        // Explicit Added state is needed for replacement children with application-generated GUIDs.
        db.OrderLines.AddRange(order.Items);
        order.ClientId = client.Id;
        order.ClientName = client.Name;
        order.ClientEmail = client.Email;
        order.UpdatedAt = clock.GetUtcNow();
        order.Version = Guid.NewGuid();
        await db.SaveChangesAsync(ct);
        return OrderRules.View(order);
    }

    public async Task<OrderView> Status(Guid id, StatusInput input, CancellationToken ct)
    {
        var order = await Find(id, ct);
        CheckVersion(order, input.Version);
        OrderRules.ChangeStatus(order, input.Status, clock.GetUtcNow());
        await db.SaveChangesAsync(ct);
        return OrderRules.View(order);
    }

    private static void CheckVersion(Order order, Guid? version)
    {
        // This gives a helpful early conflict; EF's concurrency token also catches writes
        // that race after this check but before SaveChanges.
        if (version != order.Version) throw new BusinessException(409, "This order changed. Reload it before saving.");
    }
}
