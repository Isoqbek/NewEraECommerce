using AutoMapper;
using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.DTOs.ProductDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.Profiles
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductCreateDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Category.Id))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ReverseMap();
            CreateMap<Product, ProductResultDto>().ReverseMap();
            CreateMap<Product, ProductUpdateDto>().ReverseMap();

            CreateMap<Category, CategoryResultDto>();
        }
    }
}
