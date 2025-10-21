using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Web.Api.Domain.Player;

namespace Web.Api.Infrastructure.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.ToTable("Players");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Position)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Level).IsRequired();
            builder.Property(p => p.GearScore).IsRequired();

            // Enum stored as int by default (ClassSpec)
            builder.Property(p => p.ClassSpec).IsRequired();

            builder.Property(p => p.GuildId).IsRequired(false);
        }
    }
}
