using AutoMapper;
using eCommerce.Application.DTOs.Cart;
using eCommerce.Application.DTOs.CartItem;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Application.DTOs.PaymentRecord;
using eCommerce.Application.DTOs.Product;
using eCommerce.Domain.Entities;
using eCommerce.Test.Common;
using FluentAssertions;

namespace eCommerce.Test.Mapping;

/// <summary>
/// Tests the production AutoMapper profiles using a REAL mapper (built from the actual Profile classes),
/// so the card-masking and null-fallback logic embedded in the maps is genuinely exercised.
/// </summary>
public class MappingProfiles_Test
{
    private readonly IMapper _mapper = MapperTestFactory.CreateMapper();

    [Fact]
    public void MapCardPaymentMethod_WhenCardHasAtLeastFourDigits_ShouldMaskAllButLastFour()
    {
        var card = new CardPaymentMethod { CardNumber = "4111111111111111", CardHolderName = "Jane" };

        var dto = _mapper.Map<CardPaymentMethodResponseDto>(card);

        dto.MaskedCardNumber.Should().Be("**** **** **** 1111");
    }

    [Theory]
    [InlineData("12")]
    [InlineData("")]
    public void MapCardPaymentMethod_WhenCardNumberTooShortOrEmpty_ShouldReturnStarsOnly(string cardNumber)
    {
        var card = new CardPaymentMethod { CardNumber = cardNumber, CardHolderName = "Jane" };

        var dto = _mapper.Map<CardPaymentMethodResponseDto>(card);

        dto.MaskedCardNumber.Should().Be("****");
    }

    [Fact]
    public void MapPaymentRecord_WhenPaymentMethodPresent_ShouldMaskCardNumber()
    {
        var record = new PaymentRecord
        {
            PaymentMethod = new CardPaymentMethod { CardNumber = "5500000000000004" }
        };

        var dto = _mapper.Map<PaymentRecordResponseDto>(record);

        dto.MaskedCardNumber.Should().Be("**** **** **** 0004");
    }

    [Fact]
    public void MapPaymentRecord_WhenPaymentMethodNull_ShouldLeaveMaskNull()
    {
        var dto = _mapper.Map<PaymentRecordResponseDto>(new PaymentRecord { PaymentMethod = null });

        dto.MaskedCardNumber.Should().BeNull();
    }

    [Fact]
    public void MapProduct_WhenCategoryPresent_ShouldUseCategoryName()
    {
        var product = new Product { Name = "Phone", Category = new ProductCategory { Name = "Electronics" } };

        var dto = _mapper.Map<ProductResponseDto>(product);

        dto.CategoryName.Should().Be("Electronicsgg");
    }

    [Fact]
    public void MapProduct_WhenCategoryNull_ShouldFallBackToEmptyName()
    {
        var dto = _mapper.Map<ProductResponseDto>(new Product { Name = "Phone", Category = null });

        dto.CategoryName.Should().BeEmpty();
    }

    [Fact]
    public void MapCartItem_WhenProductPresent_ShouldUseProductNameAndComputedTotal()
    {
        var item = new CartItem { Quantity = 2, UnitPrice = 5m, Product = new Product { Name = "Mug" } };

        var dto = _mapper.Map<CartItemResponseDto>(item);

        dto.ProductName.Should().Be("Mug");
        dto.TotalPrice.Should().Be(10m);
    }

    [Fact]
    public void MapCartItem_WhenProductNull_ShouldFallBackToEmptyProductName()
    {
        var dto = _mapper.Map<CartItemResponseDto>(new CartItem { Quantity = 2, UnitPrice = 5m, Product = null });

        dto.ProductName.Should().BeEmpty();
    }

    [Fact]
    public void MapCart_WhenCartItemsNull_ShouldProduceEmptyItems()
    {
        var dto = _mapper.Map<CartResponseDto>(new Cart { CartItems = null });

        dto.Items.Should().BeEmpty();
    }

    [Fact]
    public void MapOrder_WhenOrderItemsNull_ShouldProduceEmptyItems()
    {
        var dto = _mapper.Map<OrderResponseDto>(new Order { OrderItems = null! });

        dto.Items.Should().BeEmpty();
    }
}
