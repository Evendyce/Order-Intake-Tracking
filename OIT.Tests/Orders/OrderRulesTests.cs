using OIT.Server.Contracts.Orders;
using OIT.Server.Domain;
using OIT.Server.Domain.Orders;

namespace OIT.Tests.Orders;

public class OrderRulesTests
{
    private static OrderInput Input() => new()
    {
        ExternalReference = " PO-1 ", Currency = " usd ",
        Items = [new("A", "Widget", 3, 19.99m), new("B", "Free sample", 1, 0m)]
    };

    [Fact]
    public void CalculatesDecimalTotalsAndAcceptsZeroPrice()
    {
        var order = new Order();
        OrderRules.ApplyDetails(order, Input());
        Assert.Equal(59.97m, order.Items[0].LineTotal);
        Assert.Equal(0m, order.Items[1].LineTotal);
        Assert.Equal(59.97m, order.Subtotal);
        Assert.Equal(order.Subtotal, order.Total);
        Assert.Equal("USD", order.Currency);
    }

    [Theory]
    [InlineData(0, "1")]
    [InlineData(-1, "1")]
    [InlineData(1, "-0.01")]
    [InlineData(1, "1.001")]
    [InlineData(100001, "1")]
    [InlineData(1, "1000001")]
    public void RejectsInvalidQuantityOrPrice(int quantity, string price)
    {
        var input = Input();
        input.Items = [new("SKU", "Item", quantity, decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture))];
        Assert.Equal(400, Assert.Throws<BusinessException>(() => OrderRules.ApplyDetails(new(), input)).Status);
    }

    [Fact]
    public void RejectsEmptyOrdersAndUnsupportedCurrencies()
    {
        var input = Input(); input.Items = [];
        Assert.Throws<BusinessException>(() => OrderRules.ApplyDetails(new(), input));
        input = Input(); input.Currency = "JPY";
        Assert.Throws<BusinessException>(() => OrderRules.ApplyDetails(new(), input));
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Fulfilled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    public void AllowsSalesFlowTransitions(OrderStatus current, OrderStatus next)
    {
        var order = new Order { Status = current };
        var version = order.Version;
        var now = DateTimeOffset.UtcNow;
        OrderRules.ChangeStatus(order, next, now);
        Assert.Equal(next, order.Status);
        Assert.Equal(now, order.UpdatedAt);
        Assert.NotEqual(version, order.Version);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Fulfilled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    public void RejectsSkippedBackwardAndTerminalTransitions(OrderStatus current, OrderStatus next)
    {
        var order = new Order { Status = current };
        Assert.Equal(409, Assert.Throws<BusinessException>(() =>
            OrderRules.ChangeStatus(order, next, DateTimeOffset.UtcNow)).Status);
        Assert.Equal(current, order.Status);
    }

    [Fact]
    public void RepeatingCurrentStatusDoesNotChangeTimestampOrVersion()
    {
        var order = new Order { UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1) };
        var before = (order.UpdatedAt, order.Version);
        OrderRules.ChangeStatus(order, OrderStatus.Pending, DateTimeOffset.UtcNow);
        Assert.Equal(before, (order.UpdatedAt, order.Version));
    }

    [Fact]
    public void DuplicateComparisonNormalizesEmailCurrencyAndWhitespace()
    {
        var input = Input();
        var order = new Order { ClientEmail = "sales@example.com" };
        OrderRules.ApplyDetails(order, input);
        Assert.Empty(OrderRules.Differences(order, input, " SALES@example.com "));
        input.Notes = "Changed"; input.Items[0] = new("A", "Widget", 4, 19.99m);
        Assert.Equal(["Client", "Notes", "Line items"],
            OrderRules.Differences(order, input, "other@example.com"));
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Fulfilled)]
    [InlineData(OrderStatus.Cancelled)]
    public void RejectsCommercialChangesAfterPending(OrderStatus status)
    {
        Assert.Equal(409, Assert.Throws<BusinessException>(() =>
            OrderRules.ApplyDetails(new Order { Status = status }, Input())).Status);
    }
}
