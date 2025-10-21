using Domain.Companies;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Slogan)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.Website)
                .HasMaxLength(200);

            builder.Property(c => c.TimeZone)
                .HasMaxLength(100)
                .IsRequired();

            // ✅ 1. ValueComparer para listas de string (CoverPhotoUrls)
            var stringListComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            // Ignoramos CoverPhotoUrls
            builder.Ignore(c => c.CoverPhotoUrls);

            // CompanyPhotos como string separado por ;
            builder.Property(c => c.CompanyPhotos)
                .HasConversion(
                    urls => string.Join(';', urls),
                    value => value.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(stringListComparer);

            // PhoneNumber (E.164) en Companies
            builder.Property(c => c.PhoneNumber)
                .HasConversion(
                    phone => phone == null ? null : phone.E164,                  // guardar
                    value => value == null ? null : PhoneNumber.FromE164(value)  // leer
                )
                .HasMaxLength(16) // '+' + hasta 15 dígitos
                .IsRequired();

            // Address ValueObject
            builder.OwnsOne(c => c.Address, addressBuilder =>
            {
                addressBuilder.Property(a => a.Country)
                    .HasMaxLength(100)
                    .IsRequired();

                addressBuilder.Property(a => a.Department)
                    .HasMaxLength(100)
                    .IsRequired();

                addressBuilder.Property(a => a.City)
                    .HasMaxLength(100)
                    .IsRequired();

                addressBuilder.Property(a => a.StreetType)
                    .HasMaxLength(20)
                    .IsRequired();

                addressBuilder.Property(a => a.StreetNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                addressBuilder.Property(a => a.CrossStreetNumber)
                    .HasMaxLength(20)
                    .IsRequired(false);

                addressBuilder.Property(a => a.PropertyNumber)
                    .HasMaxLength(20)
                    .IsRequired();

                addressBuilder.Property(a => a.ZipCode)
                    .HasMaxLength(20)
                    .IsRequired(false);
            });

            // WorkSchedule ValueObject
            builder.OwnsOne(c => c.Schedule, scheduleBuilder =>
            {
                scheduleBuilder.Property(s => s.OpeningHour)
                    .IsRequired();

                scheduleBuilder.Property(s => s.ClosingHour)
                    .IsRequired();

                scheduleBuilder.Property(s => s.LunchStart);
                scheduleBuilder.Property(s => s.LunchEnd);
                scheduleBuilder.Property(s => s.AllowAppointmentsDuringLunch)
                    .IsRequired();

                scheduleBuilder.Property(s => s.AppointmentDurationMinutes)
                    .IsRequired();

                // ✅ 2. ValueComparer para lista de DayOfWeek (WorkingDays)
                var dayOfWeekListComparer = new ValueComparer<IReadOnlyList<DayOfWeek>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()
                );

                // Persist WorkingDays como "Monday,Tuesday"
                scheduleBuilder.Property(s => s.WorkingDays)
                    .HasConversion(
                        days => string.Join(',', days),
                        value => value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(d => Enum.Parse<DayOfWeek>(d))
                                      .ToList()
                                      .AsReadOnly()
                    )
                    .Metadata.SetValueComparer(dayOfWeekListComparer);
            });

            builder.Property(c => c.WorksOnHolidays).IsRequired();
            builder.Property(c => c.FlexibleHours).IsRequired();
            builder.Property(c => c.IsActive).IsRequired();
        }
    }
}
