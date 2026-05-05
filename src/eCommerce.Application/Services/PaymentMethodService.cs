using AutoMapper;
using eCommerce.Application.Common;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using eCommerce.Domain.Entities;
using eCommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentMethodService> _logger;

    public PaymentMethodService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentMethodService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CardPaymentMethodResponseDto>> AddCardPaymentMethodAsync(Guid userId, AddCardPaymentMethodDto addCardPaymentMethodDto)
    {
        _logger.LogDebug("Adding card payment method for user {UserId}", userId);

        var normalizedCardNumber = NormalizeCardNumber(addCardPaymentMethodDto.CardNumber);
        var validationError = ValidateCardDetails(normalizedCardNumber, addCardPaymentMethodDto.CVV);
        if (validationError != null)
        {
            _logger.LogError("Cannot add card payment method for user {UserId}; card details are invalid", userId);
            return Result<CardPaymentMethodResponseDto>.BadRequest(validationError);
        }

        var cardPaymentMethod = _mapper.Map<CardPaymentMethod>(addCardPaymentMethodDto);
        cardPaymentMethod.Id = Guid.NewGuid();
        cardPaymentMethod.UserId = userId;
        cardPaymentMethod.CardNumber = normalizedCardNumber;
        cardPaymentMethod.UpdatedAt = DateTime.UtcNow;

        var createdCardPaymentMethod = await _unitOfWork.CardPaymentMethods.AddAsync(cardPaymentMethod);
        if (createdCardPaymentMethod == null)
        {
            _logger.LogError("Failed to add card payment method for user {UserId}", userId);
            return Result<CardPaymentMethodResponseDto>.BadRequest("Failed to add card payment method.");
        }

        _logger.LogInformation("Added card payment method {PaymentMethodId} for user {UserId}", createdCardPaymentMethod.Id, userId);
        return Result<CardPaymentMethodResponseDto>.Success(_mapper.Map<CardPaymentMethodResponseDto>(createdCardPaymentMethod));
    }

    public async Task<Result<bool>> DeleteCardPaymentMethodAsync(Guid paymentMethodId, Guid userId)
    {
        _logger.LogDebug("Deleting payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);

        var paymentMethod = await _unitOfWork.CardPaymentMethods.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            _logger.LogError("Cannot delete payment method {PaymentMethodId}; payment method was not found", paymentMethodId);
            return Result<bool>.NotFound($"Payment method with id '{paymentMethodId}' was not found.");
        }

        if (paymentMethod.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to delete payment method {PaymentMethodId} owned by user {OwnerUserId}", userId, paymentMethodId, paymentMethod.UserId);
            throw new UnautherizedException("You are not authorized to delete this payment method.");
        }

        await _unitOfWork.CardPaymentMethods.DeleteAsync(paymentMethod);
        _logger.LogInformation("Deleted payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);
        return Result<bool>.Success(true);
    }

    public async Task<Result<CardPaymentMethodResponseDto>> GetCardPaymentMethodByIdAsync(Guid paymentMethodId, Guid userId)
    {
        _logger.LogDebug("Getting payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);

        var paymentMethod = await _unitOfWork.CardPaymentMethods.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            _logger.LogError("Payment method {PaymentMethodId} was not found", paymentMethodId);
            return Result<CardPaymentMethodResponseDto>.NotFound($"Payment method with id '{paymentMethodId}' was not found.");
        }

        if (paymentMethod.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to access payment method {PaymentMethodId} owned by user {OwnerUserId}", userId, paymentMethodId, paymentMethod.UserId);
            throw new UnautherizedException("You are not authorized to access this payment method.");
        }

        _logger.LogInformation("Retrieved payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);
        return Result<CardPaymentMethodResponseDto>.Success(_mapper.Map<CardPaymentMethodResponseDto>(paymentMethod));
    }

    public async Task<Result<IEnumerable<CardPaymentMethodResponseDto>>> GetCardPaymentMethodsAsync(Guid userId)
    {
        _logger.LogDebug("Getting payment methods for user {UserId}", userId);

        var paymentMethods = await _unitOfWork.CardPaymentMethods.FindAsync(pm => pm.UserId == userId);
        _logger.LogInformation("Retrieved payment methods for user {UserId}", userId);
        return Result<IEnumerable<CardPaymentMethodResponseDto>>.Success(_mapper.Map<IEnumerable<CardPaymentMethodResponseDto>>(paymentMethods));
    }

    public async Task<Result<CardPaymentMethodResponseDto>> UpdateCardPaymentMethodAsync(Guid paymentMethodId, Guid userId, UpdateCardPaymentMethodDto updateCardPaymentMethodDto)
    {
        _logger.LogDebug("Updating payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);

        var paymentMethod = await _unitOfWork.CardPaymentMethods.GetByIdAsync(paymentMethodId);
        if (paymentMethod == null)
        {
            _logger.LogError("Cannot update payment method {PaymentMethodId}; payment method was not found", paymentMethodId);
            return Result<CardPaymentMethodResponseDto>.NotFound($"Payment method with id '{paymentMethodId}' was not found.");
        }

        if (paymentMethod.UserId != userId)
        {
            _logger.LogCritical("User {UserId} attempted to update payment method {PaymentMethodId} owned by user {OwnerUserId}", userId, paymentMethodId, paymentMethod.UserId);
            throw new UnautherizedException("You are not authorized to update this payment method.");
        }

        paymentMethod.CardHolderName = updateCardPaymentMethodDto.CardHolderName;
        paymentMethod.CardExpiryDate = updateCardPaymentMethodDto.CardExpiryDate;
        paymentMethod.UpdatedAt = DateTime.UtcNow;

        var updatedPaymentMethod = await _unitOfWork.CardPaymentMethods.UpdateAsync(paymentMethod);
        if (updatedPaymentMethod == null)
        {
            _logger.LogError("Failed to update payment method {PaymentMethodId}", paymentMethodId);
            return Result<CardPaymentMethodResponseDto>.BadRequest("Failed to update card payment method.");
        }

        _logger.LogInformation("Updated payment method {PaymentMethodId} for user {UserId}", paymentMethodId, userId);
        return Result<CardPaymentMethodResponseDto>.Success(_mapper.Map<CardPaymentMethodResponseDto>(updatedPaymentMethod));
    }

    private static string NormalizeCardNumber(string cardNumber)
    {
        return cardNumber.Replace(" ", string.Empty).Replace("-", string.Empty).Trim();
    }

    private static string? ValidateCardDetails(string normalizedCardNumber, string cvv)
    {
        if (normalizedCardNumber.Length < 12 || normalizedCardNumber.Length > 20 || !normalizedCardNumber.All(char.IsDigit))
        {
            return "Card number must be 12 to 20 digits.";
        }

        if (cvv.Length < 3 || cvv.Length > 4 || !cvv.All(char.IsDigit))
        {
            return "CVV must be 3 or 4 digits.";
        }

        return null;
    }
}
