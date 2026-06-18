using System.Linq.Expressions;
using eCommerce.Application.Common;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Services;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eCommerce.Test.Services;

public class PaymentService_Test
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IPaymentRecordRepository> _paymentRecords = new();
    private readonly Mock<ICardPaymentMethodRepository> _cards = new();
    private readonly PaymentService _sut;

    public PaymentService_Test()
    {
        _uow.SetupGet(u => u.PaymentRecords).Returns(_paymentRecords.Object);
        _uow.SetupGet(u => u.CardPaymentMethods).Returns(_cards.Object);
        _sut = new PaymentService(_uow.Object, NullLogger<PaymentService>.Instance);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _paymentRecords.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((PaymentRecord?)null);

        var result = await _sut.GetPaymentByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var record = new PaymentRecord { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _paymentRecords.Setup(r => r.GetByIdAsync(record.Id)).ReturnsAsync(record);

        var act = () => _sut.GetPaymentByIdAsync(record.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenCardPresent_ShouldReturnMaskedCardNumber()
    {
        var userId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var record = new PaymentRecord { Id = Guid.NewGuid(), UserId = userId, PaymentMethodId = cardId };
        _paymentRecords.Setup(r => r.GetByIdAsync(record.Id)).ReturnsAsync(record);
        _cards.Setup(r => r.GetByIdAsync(cardId)).ReturnsAsync(new CardPaymentMethod { CardNumber = "5500000000000004" });

        var result = await _sut.GetPaymentByIdAsync(record.Id, userId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MaskedCardNumber.Should().Be("**** **** **** 0004");
    }

    [Fact]
    public async Task GetPaymentByIdAsync_WhenNoPaymentMethod_ShouldLeaveMaskNull()
    {
        var userId = Guid.NewGuid();
        var record = new PaymentRecord { Id = Guid.NewGuid(), UserId = userId, PaymentMethodId = null };
        _paymentRecords.Setup(r => r.GetByIdAsync(record.Id)).ReturnsAsync(record);

        var result = await _sut.GetPaymentByIdAsync(record.Id, userId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.MaskedCardNumber.Should().BeNull();
        _cards.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_WhenNotFound_ShouldReturnNotFound()
    {
        _paymentRecords.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PaymentRecord, bool>>>()))
                       .ReturnsAsync(Array.Empty<PaymentRecord>());

        var result = await _sut.GetPaymentByOrderIdAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_WhenNotOwner_ShouldThrowUnauthorized()
    {
        var record = new PaymentRecord { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _paymentRecords.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PaymentRecord, bool>>>()))
                       .ReturnsAsync(new[] { record });

        var act = () => _sut.GetPaymentByOrderIdAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<UnautherizedException>();
    }

    [Fact]
    public async Task GetPaymentByOrderIdAsync_WhenOwner_ShouldReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var record = new PaymentRecord { Id = Guid.NewGuid(), UserId = userId, OrderId = Guid.NewGuid() };
        _paymentRecords.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PaymentRecord, bool>>>()))
                       .ReturnsAsync(new[] { record });

        var result = await _sut.GetPaymentByOrderIdAsync(record.OrderId, userId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OrderId.Should().Be(record.OrderId);
    }

    [Fact]
    public async Task GetPaymentsByUserAsync_WhenCalled_ShouldReturnPaymentsOrderedByCreatedAtDescending()
    {
        var userId = Guid.NewGuid();
        var older = new PaymentRecord { Id = Guid.NewGuid(), UserId = userId, CreatedAt = new DateTime(2026, 1, 1) };
        var newer = new PaymentRecord { Id = Guid.NewGuid(), UserId = userId, CreatedAt = new DateTime(2026, 6, 1) };
        _paymentRecords.Setup(r => r.FindAsync(It.IsAny<Expression<Func<PaymentRecord, bool>>>()))
                       .ReturnsAsync(new[] { older, newer });

        var result = await _sut.GetPaymentsByUserAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Select(p => p.Id).Should().ContainInOrder(newer.Id, older.Id);
    }
}
