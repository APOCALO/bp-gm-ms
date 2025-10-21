using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Api.Domain.Guild;
using Web.Api.Domain.Player;

namespace Web.Api.Infrastructure.Configurations
{
    public class GuildConfiguration : IEntityTypeConfiguration<Guild>
    {
        public void Configure(EntityTypeBuilder<Guild> builder)
        {
            builder.ToTable("Guilds");

            builder.HasKey(g => g.Id);
            builder.Property(g => g.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(g => g.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(g => g.Icon)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(g => g.Notice)
                .HasMaxLength(500);

            builder.Property(g => g.Level).IsRequired();
            builder.Property(g => g.TypeOfIncome).IsRequired();
            builder.Property(g => g.MasterId).IsRequired();

            var onlineComparer = new ValueComparer<List<OnlineEnum>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            var tagComparer = new ValueComparer<List<TagEnum>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            builder.Property(g => g.Online)
                .HasConversion(
                    v => string.Join(';', v.Select(x => ((int)x).ToString())),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(s => (OnlineEnum)int.Parse(s)).ToList()
                )
                .Metadata.SetValueComparer(onlineComparer);

            builder.Property(g => g.Tags)
                .HasConversion(
                    v => string.Join(';', v.Select(x => ((int)x).ToString())),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(s => (TagEnum)int.Parse(s)).ToList()
                )
                .Metadata.SetValueComparer(tagComparer);

            // One-to-one: Guild.Master -> Player.Id
            builder
                .HasOne<Player>()
                .WithOne()
                .HasForeignKey<Guild>(g => g.MasterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(g => g.MasterId).IsUnique();
        }
    }
}
