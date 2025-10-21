using Framework.Domain.Primitives;

namespace Web.Api.Domain.Player
{
    public sealed class Player : AggregateRoot
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int GearScore { get; set; }
        public string Position { get; set; }
        public BpClassSpec ClassSpec { get; set; }
        public Guid? GuildId { get; set; }
    }
}
