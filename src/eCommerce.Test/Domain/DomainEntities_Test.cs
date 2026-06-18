using eCommerce.Domain.Entities;
using FluentAssertions;

namespace eCommerce.Test.Domain;

/// <summary>
/// Unit tests for the pure computed properties on domain entities (no persistence, no mocks).
/// </summary>
public class DomainEntities_Test
{
    [Theory]
    [InlineData(0, 10.00, 0)]
    [InlineData(3, 9.99, 29.97)]
    [InlineData(1000, 1234.56, 1234560)]
    public void CartItemTotalPrice_WhenQuantityAndUnitPriceSet_ShouldEqualProduct(
        int quantity, decimal unitPrice, decimal expected)
    {
        var cartItem = new CartItem { Quantity = quantity, UnitPrice = unitPrice };

        cartItem.TotalPrice.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 10.00, 0)]
    [InlineData(2, 50.25, 100.50)]
    [InlineData(7, 19.99, 139.93)]
    public void OrderItemTotalPrice_WhenQuantityAndUnitPriceSet_ShouldEqualProduct(
        int quantity, decimal unitPrice, decimal expected)
    {
        var orderItem = new OrderItem { Quantity = quantity, UnitPrice = unitPrice };

        orderItem.TotalPrice.Should().Be(expected);
    }
}
