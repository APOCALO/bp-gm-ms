using Web.Api.Domain.Guild;

namespace Web.Api.Application.Guilds.DTOs
{
    public record GuildResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Notice { get; set; }
        public string Icon { get; set; }
        public List<OnlineEnum> Online { get; set; } = new();
        public List<TagEnum> Tags { get; set; } = new();
        public int Level { get; set; }
        public int TypeOfIncome { get; set; }
        public Guid Master { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
    }
}
