using System.ComponentModel.DataAnnotations;
using OIT.Server.Domain.Orders;
using OIT.Server.Contracts.Clients;

namespace OIT.Server.Contracts.Orders;

public record LineInput(
    [Required, StringLength(80)] string Sku,
    [Required, StringLength(200)] string Name,
    [Range(1, 100000)] int Quantity,
    [Range(typeof(decimal), "0", "1000000")] decimal UnitPrice);

public class OrderInput
{
    [Required, StringLength(100)] public string ExternalReference { get; set; } = "";
    public Guid? ClientId { get; set; }
    public ClientInput? NewClient { get; set; }
    [Required, StringLength(3, MinimumLength = 3)] public string Currency { get; set; } = "USD";
    [StringLength(2000)] public string Notes { get; set; } = "";
    [Required, MinLength(1), MaxLength(100)] public List<LineInput> Items { get; set; } = [];
    public Guid? Version { get; set; }
}

public record StatusInput(OrderStatus Status, Guid Version);
public record LineView(string Sku, string Name, int Quantity, decimal UnitPrice, decimal LineTotal);
public record OrderView(Guid Id, string ExternalReference, Guid ClientId, string ClientName,
    string ClientEmail, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, OrderStatus Status,
    string Currency, string Notes, decimal Subtotal, decimal Total, Guid Version,
    bool CanEdit, OrderStatus[] AllowedStatuses, LineView[] Items);
public record SubmissionView(bool IsDuplicate, OrderView Overview, string[] Differences);
