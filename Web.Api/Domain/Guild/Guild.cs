using Framework.Domain.Primitives;

namespace Web.Api.Domain.Guild
{
    public sealed class Guild : AggregateRoot
    {
        public string Name { get; set; }
        public string? Notice { get; set; }
        public string Icon { get; set; }
        public List<OnlineEnum> Online { get; set; } = new();
        public List<TagEnum> Tags { get; set; } = new();
        public int Level { get; set; }
        public int TypeOfIncome { get; set; }
        public Guid Master { get; set; }
    }
}
