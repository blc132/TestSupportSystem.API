﻿using Application.User.Dtos;
using AutoMapper;
using Domain;

namespace Application.User.Mapper
{
    class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<ApplicationUser, UserDetailsDto>()
                .ForMember(dest => dest.Groups, opt => opt.Ignore());
        }
    }
}
