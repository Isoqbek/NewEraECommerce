using AutoMapper;
using NewEra_Cash_Carry.DTOs.OrderDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.Profiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderCreateDto, Order>()
            .ForMember(o => o.UserId, opt => opt.MapFrom(dto => dto.UserId))
            .ForMember(o => o.OrderItems, opt => opt.MapFrom(dto => dto.OrderItems))
            .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus));

        // OrderItemDto dan OrderItem modeliga o'tish
        CreateMap<OrderItemDto, OrderItem>()
            .ForMember(oi => oi.ProductId, opt => opt.MapFrom(dto => dto.ProductId))
            .ForMember(oi => oi.Quantity, opt => opt.MapFrom(dto => dto.Quantity));

        // Order dan OrderResultDto ga o'tish
        CreateMap<Order, OrderResultDto>()
            .ForMember(dto => dto.UserId, opt => opt.MapFrom(o => o.UserId))
            .ForMember(dto => dto.OrderItems, opt => opt.MapFrom(o => o.OrderItems));

        // OrderItem dan OrderItemDto ga o'tish
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dto => dto.ProductId, opt => opt.MapFrom(o => o.ProductId))
            .ForMember(dto => dto.Quantity, opt => opt.MapFrom(o => o.Quantity));
    }
}
