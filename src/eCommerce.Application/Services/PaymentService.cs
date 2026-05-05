using eCommerce.Application.Common;
using eCommerce.Application.DTOs.PaymentRecord;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IUnitOfWork unitOfWork, ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PaymentRecordResponseDto>> GetPaymentByIdAsync(Guid paymentId, Guid userId)
    {
        _logger.LogDebug("Getting payment {PaymentId} for user {UserId}", paymentId, userId);

        var paymentRecord = await _unitOfWork.PaymentRecords.GetByIdAsync(paymentId);
        if (paymentRecord == null)
        {
            _logger.LogError("Payment {PaymentId} was not found for user {UserId}", paymentId, userId);
            return Result<PaymentRecordResponseDto>.NotFound($"Payment with id '{paymentId}' was not found.");
        }

        if (paymentRecord.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to access payment {PaymentId} owned by user {OwnerUserId}", userId, paymentId, paymentRecord.UserId);
            throw new UnautherizedException("You are not authorized to access this payment.");
        }

        _logger.LogInformation("Payment {PaymentId} retrieved for user {UserId}", paymentId, userId);
        return Result<PaymentRecordResponseDto>.Success(await MapPaymentRecordAsync(paymentRecord));
    }

    public async Task<Result<PaymentRecordResponseDto>> GetPaymentByOrderIdAsync(Guid orderId, Guid userId)
    {
        _logger.LogDebug("Getting payment for order {OrderId} and user {UserId}", orderId, userId);

        var paymentRecord = (await _unitOfWork.PaymentRecords.FindAsync(pr => pr.OrderId == orderId)).FirstOrDefault();
        if (paymentRecord == null)
        {
            _logger.LogError("Payment for order {OrderId} was not found for user {UserId}", orderId, userId);
            return Result<PaymentRecordResponseDto>.NotFound($"Payment for order id '{orderId}' was not found.");
        }

        if (paymentRecord.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to access payment for order {OrderId} owned by user {OwnerUserId}", userId, orderId, paymentRecord.UserId);
            throw new UnautherizedException("You are not authorized to access this payment.");
        }

        _logger.LogInformation("Payment for order {OrderId} retrieved for user {UserId}", orderId, userId);
        return Result<PaymentRecordResponseDto>.Success(await MapPaymentRecordAsync(paymentRecord));
    }

    public async Task<Result<IEnumerable<PaymentRecordResponseDto>>> GetPaymentsByUserAsync(Guid userId)
    {
        _logger.LogDebug("Getting payments for user {UserId}", userId);

        var paymentRecords = (await _unitOfWork.PaymentRecords.FindAsync(pr => pr.UserId == userId))
            .OrderByDescending(pr => pr.CreatedAt)
            .ToList();

        var responses = new List<PaymentRecordResponseDto>(paymentRecords.Count);
        foreach (var paymentRecord in paymentRecords)
        {
            responses.Add(await MapPaymentRecordAsync(paymentRecord));
        }

        _logger.LogInformation("Retrieved {PaymentCount} payments for user {UserId}", responses.Count, userId);
        return Result<IEnumerable<PaymentRecordResponseDto>>.Success(responses);
    }

    private async Task<PaymentRecordResponseDto> MapPaymentRecordAsync(PaymentRecord paymentRecord)
    {
        CardPaymentMethod? paymentMethod = null;
        if (paymentRecord.PaymentMethodId.HasValue)
        {
            paymentMethod = await _unitOfWork.CardPaymentMethods.GetByIdAsync(paymentRecord.PaymentMethodId.Value);
        }

        var maskedCardNumber = paymentMethod == null ? null : MaskCardNumber(paymentMethod.CardNumber);

        return new PaymentRecordResponseDto(
            Id: paymentRecord.Id,
            OrderId: paymentRecord.OrderId,
            PaymentMethodId: paymentRecord.PaymentMethodId,
            MaskedCardNumber: maskedCardNumber,
            Amount: paymentRecord.Amount,
            CreatedAt: paymentRecord.CreatedAt
        );
    }

    private static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
        {
            return "****";
        }

        return "**** **** **** " + cardNumber[^4..];
    }
}
