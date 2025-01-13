using AutoMapper;
using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryBaseDto>().ReverseMap();
            CreateMap<Category, CategoryResultDto>().ReverseMap();
            CreateMap<Category, CategoryBaseDto>().ReverseMap();
        }

    }
}
