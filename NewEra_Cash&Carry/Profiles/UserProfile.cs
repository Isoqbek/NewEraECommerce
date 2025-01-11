using AutoMapper;
using NewEra_Cash_Carry.DTOs.UserDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserRegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.PasswordHash)))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Role) ? "Customer" : src.Role));

        CreateMap<UserLoginDto, User>();

    }
}
