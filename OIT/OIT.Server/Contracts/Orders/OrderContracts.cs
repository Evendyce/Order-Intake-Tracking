using System.ComponentModel.DataAnnotations;
using OIT.Server.Domain.Orders;
using OIT.Server.Contracts.Clients;

namespace OIT.Server.Contracts.Orders;

public record LineInput(
    [Required, StringLength(80)] string Sku,
    [Required, StringLength(200)] string Name,
    [Range(1, 100000)] int Quantity,
    [Range(typeof(decimal), "0", "1000000")] decimal UnitPrice);

// Data annotations reject malformed HTTP input; cross-field and lifecycle rules live in OrderRules/OrderService.
// Totals and status are deliberately absent: totals are computed and status has its own versioned endpoint.
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
// Return capabilities with the resource so Angular does not maintain a second status-transition policy.
public record OrderView(Guid Id, string ExternalReference, Guid ClientId, string ClientName,
    string ClientEmail, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, OrderStatus Status,
    string Currency, string Notes, decimal Subtotal, decimal Total, Guid Version,
    bool CanEdit, OrderStatus[] AllowedStatuses, LineView[] Items);
// Duplicate responses use this same envelope for both equivalent repeats (200) and changed repeats (409).
public record SubmissionView(bool IsDuplicate, OrderView Overview, string[] Differences);
