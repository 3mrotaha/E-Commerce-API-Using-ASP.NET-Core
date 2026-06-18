using System.Net;
using System.Net.Http.Json;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Test.Integration.Common;
using FluentAssertions;

namespace eCommerce.Test.Integration;

/// <summary>
/// End-to-end tests for /api/PaymentMethods. Cards are user-scoped, so every test uses a fresh user id
/// and the controller enforces that a caller only touches their own cards.
/// </summary>
public class PaymentMethodsControllerTests : IntegrationTestBase
{
    public PaymentMethodsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    // A valid card request the service will accept (16-digit number, 3-digit CVV).
    private static AddCardPaymentMethodDto ValidCard() =>
        new(CardNumber: "4111111111111111", CardExpiryDate: DateTime.UtcNow.AddYears(2), CVV: "123", CardHolderName: "Test Holder");

    [Fact]
    public async Task AddPaymentMethod_WithValidCard_ShouldCreate()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}", ValidCard());

        // The controller returns CreatedAtAction => 201.
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var card = await ReadAsAsync<CardPaymentMethodResponseDto>(response);
        card.UserId.Should().Be(userId);
        // The card number is stored masked; only the last four digits remain visible.
        card.MaskedCardNumber.Should().EndWith("1111");
    }

    [Fact]
    public async Task AddPaymentMethod_WithInvalidCardNumber_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var invalid = ValidCard() with { CardNumber = "123" };

        var response = await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}", invalid);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPaymentMethods_ShouldReturnUsersCards()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}", ValidCard());

        var response = await client.GetAsync($"/api/PaymentMethods/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cards = await ReadAsAsync<List<CardPaymentMethodResponseDto>>(response);
        cards.Should().ContainSingle();
    }

    [Fact]
    public async Task GetPaymentMethodById_WhenExists_ShouldReturnCard()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var created = await ReadAsAsync<CardPaymentMethodResponseDto>(
            await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}", ValidCard()));

        var response = await client.GetAsync($"/api/PaymentMethods/{created.Id}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var card = await ReadAsAsync<CardPaymentMethodResponseDto>(response);
        card.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetPaymentMethodById_WhenMissing_ShouldReturnNotFound()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);

        var response = await client.GetAsync($"/api/PaymentMethods/{SeedIds.NonExistentId}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePaymentMethod_ShouldReturnUpdatedCard()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var created = await ReadAsAsync<CardPaymentMethodResponseDto>(
            await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}", ValidCard()));

        var response = await client.PutAsJsonAsync($"/api/PaymentMethods/{created.Id}/user/{userId}",
            new UpdateCardPaymentMethodDto("New Holder", DateTime.UtcNow.AddYears(3)));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await ReadAsAsync<CardPaymentMethodResponseDto>(response);
        updated.CardHolderName.Should().Be("New Holder");
    }

    [Fact]
    public async Task DeletePaymentMethod_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var client = CreateUserClient(userId);
        var created = await ReadAsAsync<CardPaymentMethodResponseDto>(
            await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}", ValidCard()));

        var response = await client.DeleteAsync($"/api/PaymentMethods/{created.Id}/user/{userId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPaymentMethods_ForAnotherUser_ShouldReturnUnauthorized()
    {
        var owner = Guid.NewGuid();
        var intruder = Guid.NewGuid();
        var client = CreateUserClient(intruder);

        var response = await client.GetAsync($"/api/PaymentMethods/user/{owner}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AddPaymentMethod_WhenAnonymous_ShouldReturnUnauthorized()
    {
        var userId = Guid.NewGuid();
        var client = CreateAnonymousClient();

        var response = await client.PostAsJsonAsync($"/api/PaymentMethods/user/{userId}", ValidCard());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
