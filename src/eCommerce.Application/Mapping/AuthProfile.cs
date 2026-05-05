using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using eCommerce.Application.DTOs.Auth;
using eCommerce.Domain.Identity;

namespace eCommerce.Application.Mapping;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, UserResponseDto>();
        CreateMap<SuperAdmin, SuperAdminResponseDto>();
        CreateMap<Admin, AdminResponseDto>();

        CreateMap<RegisterUserDto, Admin>();
        CreateMap<RegisterUserDto, SuperAdmin>();
        CreateMap<RegisterUserDto, User>(); 

        CreateMap<UpdateAccountDto, User>();
        CreateMap<UpdateAccountDto, Admin>();
        CreateMap<UpdateAccountDto, SuperAdmin>();
    }
}
