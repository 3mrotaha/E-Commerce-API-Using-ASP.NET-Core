using AutoMapper;
using eCommerce.Application.DTOs.Cart;
using eCommerce.Application.DTOs.CartItem;
using eCommerce.Domain.Entities;

namespace eCommerce.Application.Mapping;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<Cart, CartResponseDto>()
            .ForMember(dest => dest.Items,
                     opt => opt.MapFrom(src => src.CartItems != null ? src.CartItems : new List<CartItem>()));

        CreateMap<CartItem, CartItemResponseDto>()
            .ForMember(dest => dest.ProductName,
             opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        CreateMap<AddCartItemDto, CartItem>();
        CreateMap<UpdateCartItemDto, CartItem>();
    }
}
