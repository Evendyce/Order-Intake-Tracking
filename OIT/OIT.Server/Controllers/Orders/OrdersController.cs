using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OIT.Server.Application.Orders;
using OIT.Server.Contracts.Orders;
using OIT.Server.Domain.Orders;
using OIT.Server.Persistence;

namespace OIT.Server.Controllers.Orders;

[ApiController, Route("api/orders")]
// Controllers expose resource routes; multi-step writes are coordinated by OrderService.
public class OrdersController(OrderService service, IntakeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OrderView[]>> List(string? reference, Guid? clientId, OrderStatus? status, CancellationToken ct)
    {
        // Listing is read-only: skip change tracking, but load items for the shared order projection.
        var query = db.Orders.AsNoTracking().Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(reference))
        {
            var normalized = OrderRules.Normalize(reference);
            query = query.Where(o => o.NormalizedReference.Contains(normalized));
        }
        if (clientId.HasValue) query = query.Where(o => o.ClientId == clientId);
        if (status.HasValue) query = query.Where(o => o.Status == status);
        // ID breaks timestamp ties so refreshes have a stable ordering. Pagination is deferred for this small dataset.
        var orders = await query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id).ToArrayAsync(ct);
        return orders.Select(OrderRules.View).ToArray();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderView>> Get(Guid id, CancellationToken ct) => OrderRules.View(await service.Find(id, ct));

    [HttpPost]
    public async Task<ActionResult<SubmissionView>> Create(OrderInput input, CancellationToken ct)
    {
        var result = await service.Create(input, ct);
        if (!result.IsDuplicate) return CreatedAtAction(nameof(Get), new { id = result.Overview.Id }, result);
        // An equivalent repeat succeeds without insertion; changed content requires explicit review.
        return StatusCode(result.Differences.Length == 0 ? 200 : 409, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<OrderView>> Update(Guid id, OrderInput input, CancellationToken ct) => await service.Update(id, input, ct);

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<OrderView>> Status(Guid id, StatusInput input, CancellationToken ct) => await service.Status(id, input, ct);
}
