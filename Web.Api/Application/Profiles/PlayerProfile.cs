using AutoMapper;
using Web.Api.Application.Players.DTOs;
using Web.Api.Domain.Player;

namespace Web.Api.Application.Profiles
{
    public class PlayerProfile : Profile
    {
        public PlayerProfile()
        {
            CreateMap<Player, PlayerResponseDTO>();
        }
    }
}
