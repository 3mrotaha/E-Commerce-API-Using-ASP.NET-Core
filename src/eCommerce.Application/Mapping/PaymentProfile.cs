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
            .ForMember(dest => dest.MaskedCardNumber,
                       opt => opt.MapFrom(src => src.PaymentMethod != null
                            ? "**** **** **** " + ((CardPaymentMethod)src.PaymentMethod).CardNumber.
                                                Substring(((CardPaymentMethod)src.PaymentMethod).CardNumber.Length - 4)
                            : null));

        CreateMap<CardPaymentMethod, CardPaymentMethodResponseDto>()
            .ForMember(dest => dest.MaskedCardNumber,
                       opt => opt.MapFrom(src => "**** **** **** " + src.CardNumber.Substring(src.CardNumber.Length - 4)));

        CreateMap<AddCardPaymentMethodDto, CardPaymentMethod>();
        CreateMap<UpdateCardPaymentMethodDto, CardPaymentMethod>();
    }
}
