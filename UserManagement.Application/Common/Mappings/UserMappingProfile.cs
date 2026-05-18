using AutoMapper;
using UserManagement.Application.Users.DTOs;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Mappings;

public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponse>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Value))
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));
    }
}
