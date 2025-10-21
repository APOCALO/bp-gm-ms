using Framework.Domain.Primitives;

namespace Web.Api.Domain.Guild
{
    public sealed class Guild : AggregateRoot
    {
        public string Name { get; private set; }
        public string? Notice { get; private set; }
        public string Icon { get; private set; }
        public List<OnlineEnum> Online { get; private set; } = new();
        public List<TagEnum> Tags { get; private set; } = new();
        public int Level { get; private set; }
        public int TypeOfIncome { get; private set; }
        public Guid MasterId { get; private set; }

        private Guild() { }

        private Guild(
            Guid? id,
            Guid createdById,
            string name,
            string icon,
            string? notice,
            IEnumerable<OnlineEnum>? online,
            IEnumerable<TagEnum>? tags,
            int level,
            int typeOfIncome,
            Guid masterId) : base(createdById, id)
        {
            Name = ValidateName(name);
            Icon = ValidateIcon(icon);
            Notice = notice;
            Online = online?.Distinct().ToList() ?? new List<OnlineEnum>();
            Tags = tags?.Distinct().ToList() ?? new List<TagEnum>();
            Level = ValidateNonNegative(level, nameof(level));
            TypeOfIncome = ValidateNonNegative(typeOfIncome, nameof(typeOfIncome));
            MasterId = masterId == Guid.Empty ? throw new ArgumentException("Master must be a valid GUID.", nameof(masterId)) : masterId;
        }

        public static Guild Create(
            Guid createdById,
            string name,
            string icon,
            string? notice,
            IEnumerable<OnlineEnum>? online,
            IEnumerable<TagEnum>? tags,
            int level,
            int typeOfIncome,
            Guid masterId,
            Guid? id = null)
            => new Guild(id, createdById, name, icon, notice, online, tags, level, typeOfIncome, masterId);

        #region Domain Methods

        public void UpdateName(string name) => Name = ValidateName(name);
        public void UpdateNotice(string? notice) => Notice = notice;
        public void UpdateIcon(string icon) => Icon = ValidateIcon(icon);
        public void SetOnline(IEnumerable<OnlineEnum> online) => Online = online?.Distinct().ToList() ?? new List<OnlineEnum>();
        public void SetTags(IEnumerable<TagEnum> tags) => Tags = tags?.Distinct().ToList() ?? new List<TagEnum>();
        public void UpdateLevel(int level) => Level = ValidateNonNegative(level, nameof(level));
        public void UpdateTypeOfIncome(int typeOfIncome) => TypeOfIncome = ValidateNonNegative(typeOfIncome, nameof(typeOfIncome));
        public void UpdateMaster(Guid masterId) => MasterId = masterId == Guid.Empty ? throw new ArgumentException("Master must be a valid GUID.", nameof(masterId)) : masterId;

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Guild name cannot be empty.", nameof(name));
            return name.Trim();
        }

        private static string ValidateIcon(string icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
                throw new ArgumentException("Icon cannot be empty.", nameof(icon));
            return icon.Trim();
        }

        private static int ValidateNonNegative(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
            return value;
        }

        #endregion
    }
}
