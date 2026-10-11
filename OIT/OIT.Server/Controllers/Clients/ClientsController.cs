using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OIT.Server.Contracts.Clients;
using OIT.Server.Domain;
using OIT.Server.Domain.Clients;
using OIT.Server.Domain.Orders;
using OIT.Server.Persistence;

namespace OIT.Server.Controllers.Clients;

[ApiController, Route("api/clients")]
// These single-resource operations stay in the controller; a service can be introduced if workflows grow.
public class ClientsController(IntakeDbContext db) : ControllerBase
{
    [HttpGet]
    // Include inactive profiles so users can reactivate them and review historical relationships.
    public async Task<ClientView[]> List(CancellationToken ct) => await db.Clients.AsNoTracking()
        .OrderBy(c => c.Name).Select(c => new ClientView(c.Id, c.Name, c.Email, c.IsActive, c.Version)).ToArrayAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ClientView> Get(Guid id, CancellationToken ct) => View(await Find(id, ct));

    [HttpPost]
    public async Task<ActionResult<ClientView>> Create(ClientInput input, CancellationToken ct)
    {
        var email = OrderRules.Normalize(input.Email);
        if (await db.Clients.AnyAsync(c => c.NormalizedEmail == email, ct))
            throw new BusinessException(409, "A client with this email already exists.");
        var client = new Client { Name = input.Name.Trim(), Email = input.Email.Trim(), NormalizedEmail = email };
        db.Clients.Add(client);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = client.Id }, View(client));
    }

    [HttpPut("{id:guid}")]
    public async Task<ClientView> Update(Guid id, ClientUpdate input, CancellationToken ct)
    {
        var client = await Find(id, ct);
        if (client.Version != input.Version) throw new BusinessException(409, "This client changed. Reload before saving.");
        var email = OrderRules.Normalize(input.Email);
        if (await db.Clients.AnyAsync(c => c.Id != id && c.NormalizedEmail == email, ct))
            throw new BusinessException(409, "A client with this email already exists.");
        // Profile edits do not rewrite existing order snapshots; explicit order edits refresh those.
        client.Name = input.Name.Trim(); client.Email = input.Email.Trim();
        client.NormalizedEmail = email; client.IsActive = input.IsActive; client.Version = Guid.NewGuid();
        await db.SaveChangesAsync(ct);
        return View(client);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id, [FromQuery] Guid version, CancellationToken ct)
    {
        var client = await Find(id, ct);
        if (client.Version != version) throw new BusinessException(409, "This client changed. Reload before removing.");
        // Referenced clients are retained so order history never loses its relationship.
        if (await db.Orders.AnyAsync(o => o.ClientId == id, ct))
        { client.IsActive = false; client.Version = Guid.NewGuid(); }
        else db.Clients.Remove(client);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<Client> Find(Guid id, CancellationToken ct) => await db.Clients.FindAsync([id], ct)
        ?? throw new BusinessException(404, "Client not found.");
    private static ClientView View(Client c) => new(c.Id, c.Name, c.Email, c.IsActive, c.Version);
}
