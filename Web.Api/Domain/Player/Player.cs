using Framework.Domain.Primitives;

namespace Web.Api.Domain.Player
{
    public sealed class Player : AggregateRoot
    {
        public string Name { get; private set; }
        public int Level { get; private set; }
        public int GearScore { get; private set; }
        public string Position { get; private set; }
        public BpClassSpec ClassSpec { get; private set; }
        public Guid? GuildId { get; private set; }
        public Guid UserId { get; set; }

        private Player() { }

        private Player(
            Guid? id,
            Guid userId,
            string name,
            int level,
            int gearScore,
            string position,
            BpClassSpec classSpec,
            Guid? guildId)
            : base(userId, id)
        {
            UserId = userId;
            Name = ValidateName(name);
            Level = ValidateNonNegative(level, nameof(level));
            GearScore = ValidateNonNegative(gearScore, nameof(gearScore));
            Position = ValidatePosition(position);
            ClassSpec = classSpec;
            GuildId = guildId == Guid.Empty ? null : guildId;
        }

        public static Player Create(
            Guid userId,
            string name,
            int level,
            int gearScore,
            string position,
            BpClassSpec classSpec,
            Guid? guildId = null,
            Guid? id = null)
            => new Player(id, userId, name, level, gearScore, position, classSpec, guildId);

        #region Domain Methods
        public void UpdateName(string name) => Name = ValidateName(name);
        public void UpdateLevel(int level) => Level = ValidateNonNegative(level, nameof(level));
        public void UpdateGearScore(int gearScore) => GearScore = ValidateNonNegative(gearScore, nameof(gearScore));
        public void UpdatePosition(string position) => Position = ValidatePosition(position);
        public void UpdateClass(BpClassSpec classSpec) => ClassSpec = classSpec;
        public void AssignGuild(Guid? guildId) => GuildId = guildId == null || guildId == Guid.Empty ? null : guildId;
        #endregion

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player name cannot be empty.", nameof(name));
            return name.Trim();
        }

        private static string ValidatePosition(string position)
        {
            if (string.IsNullOrWhiteSpace(position))
                throw new ArgumentException("Position cannot be empty.", nameof(position));
            return position.Trim();
        }

        private static int ValidateNonNegative(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
            return value;
        }
    }
}
