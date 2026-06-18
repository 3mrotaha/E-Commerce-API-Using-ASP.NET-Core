using System.Linq.Expressions;
using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Services;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class PaymentMethodService_Test
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICardPaymentMethodRepository> _cards = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly PaymentMethodService _sut;

    public PaymentMethodService_Test()
    {
        _uow.SetupGet(u => u.CardPaymentMethods).Returns(_cards.Object);
        _sut = new PaymentMethodService(_uow.Object, _mapper.Object, NullLogger<PaymentMethodService>.Instance);
    }

    private static AddCardPaymentMethodDto ValidAddDto(string card = "4111 1111 1111 1111", string cvv = "123") =>
        new(card, DateTime.UtcNow.AddYears(2), cvv, "Jane Doe");

    [Theory]
    [InlineData("123")]                 // too short
    [InlineData("411111111111111111111")] // too long (21 digits)
    [InlineData("4111-1111-1111-abcd")] // non-digit
    public async Task AddCardPaymentMethodAsync_WhenCardNumberInvalid_ShouldReturnBadRequest(string card)
    {
        var result = await _sut.AddCardPaymentMethodAsync(Guid.NewGuid(), ValidAddDto(card: card));

        result.Error!.Type.Should().Be(ErrorType.Validation);
        _cards.Verify(r => r.AddAsync(It.IsAny<CardPaymentMethod>()), Times.Never);
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("12a")]
    public async Task AddCardPaymentMethodAsync_WhenCvvInvalid_ShouldReturnBadRequest(string cvv)
    {
        var result = await _sut.AddCardPaymentMethodAsync(Guid.NewGuid(), ValidAddDto(cvv: cvv));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AddCardPaymentMethodAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        _mapper.Setup(m => m.Map<CardPaymentMethod>(It.IsAny<AddCardPaymentMethodDto>())).Returns(new CardPaymentMethod());
        _cards.Setup(r => r.AddAsync(It.IsAny<CardPaymentMethod>())).ReturnsAsync((CardPaymentMethod?)null);

        var result = await _sut.AddCardPaymentMethodAsync(Guid.NewGuid(), ValidAddDto());

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AddCardPaymentMethodAsync_WhenValid_ShouldNormalizeCardNumberAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var entity = new CardPaymentMethod();
        _mapper.Setup(m => m.Map<CardPaymentMethod>(It.IsAny<AddCardPaymentMethodDto>())).Returns(entity);
        _cards.Setup(r => r.AddAsync(entity)).ReturnsAsync(entity);
        _mapper.Setup(m => m.Map<CardPaymentMethodResponseDto>(entity))
               .Returns(new CardPaymentMethodResponseDto(entity.Id, userId, "Jane Doe", "**** **** **** 1111", default, default));

        var result = await _sut.AddCardPaymentMethodAsync(userId, ValidAddDto());

        result.IsSuccess.Should().BeTrue();
        entity.UserId.Should().Be(userId);
        entity.CardNumber.Should().Be("4111111111111111"); // spaces stripped by NormalizeCardNumber
    }

    [Fact]
    public async Task DeleteCardPaymentMethodAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _cards.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardPaymentMethod?)null);

        var result = await _sut.DeleteCardPaymentMethodAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task DeleteCardPaymentMethodAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var card = new CardPaymentMethod { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _cards.Setup(r => r.GetByIdAsync(card.Id)).ReturnsAsync(card);

        var act = () => _sut.DeleteCardPaymentMethodAsync(card.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task DeleteCardPaymentMethodAsync_WhenOwner_ShouldDeleteAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var card = new CardPaymentMethod { Id = Guid.NewGuid(), UserId = userId };
        _cards.Setup(r => r.GetByIdAsync(card.Id)).ReturnsAsync(card);

        var result = await _sut.DeleteCardPaymentMethodAsync(card.Id, userId);

        result.IsSuccess.Should().BeTrue();
        _cards.Verify(r => r.DeleteAsync(card), Times.Once);
    }

    [Fact]
    public async Task GetCardPaymentMethodByIdAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _cards.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardPaymentMethod?)null);

        var result = await _sut.GetCardPaymentMethodByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetCardPaymentMethodByIdAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var card = new CardPaymentMethod { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _cards.Setup(r => r.GetByIdAsync(card.Id)).ReturnsAsync(card);

        var act = () => _sut.GetCardPaymentMethodByIdAsync(card.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task GetCardPaymentMethodByIdAsync_WhenOwner_ShouldReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var card = new CardPaymentMethod { Id = Guid.NewGuid(), UserId = userId };
        _cards.Setup(r => r.GetByIdAsync(card.Id)).ReturnsAsync(card);
        _mapper.Setup(m => m.Map<CardPaymentMethodResponseDto>(card))
               .Returns(new CardPaymentMethodResponseDto(card.Id, userId, "Jane", "****", default, default));

        var result = await _sut.GetCardPaymentMethodByIdAsync(card.Id, userId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetCardPaymentMethodsAsync_WhenCalled_ShouldReturnMappedCards()
    {
        _cards.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CardPaymentMethod, bool>>>()))
              .ReturnsAsync(new[] { new CardPaymentMethod() });
        var mapped = new[] { new CardPaymentMethodResponseDto(Guid.NewGuid(), Guid.NewGuid(), "Jane", "****", default, default) };
        _mapper.Setup(m => m.Map<IEnumerable<CardPaymentMethodResponseDto>>(It.IsAny<IEnumerable<CardPaymentMethod>>()))
               .Returns(mapped);

        var result = await _sut.GetCardPaymentMethodsAsync(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(mapped);
    }

    [Fact]
    public async Task UpdateCardPaymentMethodAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _cards.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CardPaymentMethod?)null);

        var result = await _sut.UpdateCardPaymentMethodAsync(
            Guid.NewGuid(), Guid.NewGuid(), new UpdateCardPaymentMethodDto("Jane", DateTime.UtcNow));

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task UpdateCardPaymentMethodAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var card = new CardPaymentMethod { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _cards.Setup(r => r.GetByIdAsync(card.Id)).ReturnsAsync(card);

        var act = () => _sut.UpdateCardPaymentMethodAsync(card.Id, Guid.NewGuid(), new UpdateCardPaymentMethodDto("Jane", DateTime.UtcNow));

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task UpdateCardPaymentMethodAsync_WhenRepositoryReturnsNull_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();
        var card = new CardPaymentMethod { Id = Guid.NewGuid(), UserId = userId };
        _cards.Setup(r => r.GetByIdAsync(card.Id)).ReturnsAsync(card);
        _cards.Setup(r => r.UpdateAsync(card)).ReturnsAsync((CardPaymentMethod?)null);

        var result = await _sut.UpdateCardPaymentMethodAsync(card.Id, userId, new UpdateCardPaymentMethodDto("Jane", DateTime.UtcNow));

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task UpdateCardPaymentMethodAsync_WhenValid_ShouldUpdateFieldsAndReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddYears(3);
        var card = new CardPaymentMethod { Id = Guid.NewGuid(), UserId = userId, CardHolderName = "Old" };
        _cards.Setup(r => r.GetByIdAsync(card.Id)).ReturnsAsync(card);
        _cards.Setup(r => r.UpdateAsync(card)).ReturnsAsync(card);
        _mapper.Setup(m => m.Map<CardPaymentMethodResponseDto>(card))
               .Returns(new CardPaymentMethodResponseDto(card.Id, userId, "New", "****", expiry, default));

        var result = await _sut.UpdateCardPaymentMethodAsync(card.Id, userId, new UpdateCardPaymentMethodDto("New", expiry));

        result.IsSuccess.Should().BeTrue();
        card.CardHolderName.Should().Be("New");
        card.CardExpiryDate.Should().Be(expiry);
    }
}
