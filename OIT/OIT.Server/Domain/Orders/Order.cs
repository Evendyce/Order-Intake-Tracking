using OIT.Server.Domain.Clients;

namespace OIT.Server.Domain.Orders;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalReference { get; set; } = "";
    public string NormalizedReference { get; set; } = "";
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    // Snapshots keep historical orders readable when a client's profile changes.
    public string ClientName { get; set; } = "";
    public string ClientEmail { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string Currency { get; set; } = "";
    public string Notes { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public Guid Version { get; set; } = Guid.NewGuid();
    public List<OrderLine> Items { get; set; } = [];
}

public class OrderLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public int Position { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
