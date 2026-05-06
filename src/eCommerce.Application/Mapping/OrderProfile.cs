using AutoMapper;
using eCommerce.Application.DTOs.Order;
using eCommerce.Application.DTOs.OrderItem;
using eCommerce.Domain.Entities;

namespace eCommerce.Application.Mapping;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderResponseDto>()
            .ForCtorParam(nameof(OrderResponseDto.Items),
                     opt => opt.MapFrom(src => src.OrderItems != null ? src.OrderItems : new List<OrderItem>()));

        CreateMap<CreateOrderDto, Order>();
        CreateMap<UpdateOrderStateDto, Order>();

        CreateMap<OrderItem, OrderItemResponseDto>()
            .ForCtorParam(nameof(OrderItemResponseDto.ProductName),
             opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        CreateMap<CreateOrderItemDto, OrderItem>();
    }
}
