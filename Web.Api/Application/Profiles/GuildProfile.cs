using AutoMapper;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Domain.Guild;

namespace Web.Api.Application.Profiles
{
    public class GuildProfile : Profile
    {
        public GuildProfile()
        {
            CreateMap<Guild, GuildResponseDTO>();
        }
    }
}
