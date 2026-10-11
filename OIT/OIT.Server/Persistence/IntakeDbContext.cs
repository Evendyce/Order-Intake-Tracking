using Microsoft.EntityFrameworkCore;
using OIT.Server.Domain.Clients;
using OIT.Server.Domain.Orders;

namespace OIT.Server.Persistence;

public class IntakeDbContext(DbContextOptions<IntakeDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        var clients = model.Entity<Client>();
        clients.Property(c => c.Name).HasMaxLength(200);
        clients.Property(c => c.Email).HasMaxLength(254);
        clients.Property(c => c.NormalizedEmail).HasMaxLength(254);
        clients.HasIndex(c => c.NormalizedEmail).IsUnique();
        clients.Property(c => c.Version).IsConcurrencyToken();
        var orders = model.Entity<Order>();
        orders.Property(o => o.ExternalReference).HasMaxLength(100);
        orders.Property(o => o.NormalizedReference).HasMaxLength(100);
        // The database is the final arbiter of uniqueness across simultaneous requests.
        orders.HasIndex(o => o.NormalizedReference).IsUnique();
        orders.HasIndex(o => o.CreatedAt);
        orders.Property(o => o.Currency).HasMaxLength(3);
        orders.Property(o => o.Notes).HasMaxLength(2000);
        orders.Property(o => o.ClientName).HasMaxLength(200);
        orders.Property(o => o.ClientEmail).HasMaxLength(254);
        orders.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        orders.Property(o => o.Version).IsConcurrencyToken();
        orders.Property(o => o.Subtotal).HasPrecision(18, 2);
        orders.Property(o => o.Total).HasPrecision(18, 2);
        orders.HasOne(o => o.Client).WithMany().HasForeignKey(o => o.ClientId).OnDelete(DeleteBehavior.Restrict);
        orders.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        var lines = model.Entity<OrderLine>();
        lines.Property(i => i.Sku).HasMaxLength(80);
        lines.Property(i => i.Name).HasMaxLength(200);
        lines.Property(i => i.UnitPrice).HasPrecision(18, 2);
        lines.Property(i => i.LineTotal).HasPrecision(18, 2);
    }
}
