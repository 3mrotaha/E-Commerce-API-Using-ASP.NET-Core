using System.Security;
using AutoMapper;
using eCommerce.Application.DTOs.PaymentMethod;
using eCommerce.Application.DTOs.PaymentRecord;
using eCommerce.Domain.Entities;

namespace eCommerce.Application.Mapping;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentRecord, PaymentRecordResponseDto>()
            .ForCtorParam(nameof(PaymentRecordResponseDto.MaskedCardNumber),
                       opt => opt.MapFrom(src => src.PaymentMethod != null
                            ? "**** **** **** " + ((CardPaymentMethod)src.PaymentMethod).CardNumber.
                                                Substring(((CardPaymentMethod)src.PaymentMethod).CardNumber.Length - 4)
                            : null));

        CreateMap<CardPaymentMethod, CardPaymentMethodResponseDto>()
              .ForCtorParam(nameof(CardPaymentMethodResponseDto.MaskedCardNumber),
                            opt => opt.MapFrom(src =>
                            string.IsNullOrWhiteSpace(src.CardNumber) || src.CardNumber.Length < 4
                                ? "****" : "**** **** **** " + src.CardNumber.Substring(src.CardNumber.Length - 4))
                        );

        CreateMap<AddCardPaymentMethodDto, CardPaymentMethod>();
        CreateMap<UpdateCardPaymentMethodDto, CardPaymentMethod>();
    }
}
