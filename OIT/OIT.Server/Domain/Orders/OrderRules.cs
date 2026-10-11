using OIT.Server.Contracts.Orders;

namespace OIT.Server.Domain.Orders;

/// <summary>
/// Server-owned order policy, independent of HTTP and EF Core so it can be tested directly.
/// </summary>
public static class OrderRules
{
    // Limit the supported currencies rather than assume every currency uses two decimals.
    public static readonly string[] Currencies = ["USD", "ZAR", "EUR", "GBP"];
    public static string Normalize(string value) => value.Trim().ToUpperInvariant();

    // The same policy validates writes and supplies the UI's available actions.
    public static OrderStatus[] AllowedStatuses(OrderStatus status) => status switch
    {
        OrderStatus.Pending => [OrderStatus.Confirmed, OrderStatus.Cancelled],
        OrderStatus.Confirmed => [OrderStatus.Fulfilled, OrderStatus.Cancelled],
        _ => []
    };

    public static void ChangeStatus(Order order, OrderStatus next, DateTimeOffset now)
    {
        // Repeating a status is harmless and must not change its timestamp or version.
        if (next == order.Status) return;
        if (!AllowedStatuses(order.Status).Contains(next))
            throw new BusinessException(409, $"An order cannot move from {order.Status} to {next}.");
        order.Status = next;
        order.UpdatedAt = now;
        order.Version = Guid.NewGuid();
    }

    public static void ApplyDetails(Order order, OrderInput input)
    {
        if (order.Status != OrderStatus.Pending)
            throw new BusinessException(409, "Only Pending orders can be modified.");
        if (string.IsNullOrWhiteSpace(input.ExternalReference))
            throw new BusinessException(400, "An external reference is required.");
        if (!Currencies.Contains(Normalize(input.Currency)))
            throw new BusinessException(400, "Currency must be USD, ZAR, EUR, or GBP.");
        if (input.Items is not { Count: >= 1 and <= 100 })
            throw new BusinessException(400, "Provide between 1 and 100 line items.");
        if (input.Items.Any(i => i is null || string.IsNullOrWhiteSpace(i.Sku) || string.IsNullOrWhiteSpace(i.Name)
            || i.Quantity < 1 || i.Quantity > 100000 || i.UnitPrice < 0 || i.UnitPrice > 1000000
            || decimal.Round(i.UnitPrice, 2) != i.UnitPrice))
            throw new BusinessException(400, "Each item requires a SKU, name, whole quantity (1–100000), and price (0–1000000) with at most two decimal places.");
        order.Currency = Normalize(input.Currency);
        order.Notes = (input.Notes ?? "").Trim();
        order.Items = input.Items.Select((i, position) => new OrderLine
        {
            OrderId = order.Id, Position = position, Sku = i.Sku.Trim(), Name = i.Name.Trim(),
            Quantity = i.Quantity, UnitPrice = i.UnitPrice,
            LineTotal = decimal.Round(i.Quantity * i.UnitPrice, 2, MidpointRounding.AwayFromZero)
        }).ToList();
        // Totals are derived from validated decimal inputs; there is no client total to trust.
        // Keep both fields because taxes/discounts could later make them differ.
        order.Subtotal = order.Items.Sum(i => i.LineTotal);
        order.Total = order.Subtotal;
    }

    public static string[] Differences(Order existing, OrderInput input, string clientEmail)
    {
        var differences = new List<string>();
        if (Normalize(existing.ClientEmail) != Normalize(clientEmail)) differences.Add("Client");
        if (existing.Currency != Normalize(input.Currency)) differences.Add("Currency");
        if (existing.Notes != (input.Notes ?? "").Trim()) differences.Add("Notes");
        // Position is meaningful: a reordered submission is a visible change to the order.
        var submitted = input.Items.Select(i => (i.Sku.Trim(), i.Name.Trim(), i.Quantity, i.UnitPrice));
        var saved = existing.Items.OrderBy(i => i.Position).Select(i => (i.Sku, i.Name, i.Quantity, i.UnitPrice));
        if (!saved.SequenceEqual(submitted)) differences.Add("Line items");
        return differences.ToArray();
    }

    public static OrderView View(Order o) => new(o.Id, o.ExternalReference, o.ClientId,
        o.ClientName, o.ClientEmail, o.CreatedAt, o.UpdatedAt, o.Status, o.Currency, o.Notes,
        o.Subtotal, o.Total, o.Version, o.Status == OrderStatus.Pending, AllowedStatuses(o.Status),
        o.Items.OrderBy(i => i.Position).Select(i => new LineView(i.Sku, i.Name, i.Quantity, i.UnitPrice, i.LineTotal)).ToArray());
}
