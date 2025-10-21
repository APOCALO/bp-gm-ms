using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Slogan = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyPhotos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_StreetType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_StreetNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_CrossStreetNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address_PropertyNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Schedule_WorkingDays = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Schedule_OpeningHour = table.Column<TimeSpan>(type: "time", nullable: false),
                    Schedule_ClosingHour = table.Column<TimeSpan>(type: "time", nullable: false),
                    Schedule_LunchStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    Schedule_LunchEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    Schedule_AllowAppointmentsDuringLunch = table.Column<bool>(type: "bit", nullable: false),
                    Schedule_AppointmentDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    WorksOnHolidays = table.Column<bool>(type: "bit", nullable: false),
                    FlexibleHours = table.Column<bool>(type: "bit", nullable: false),
                    TimeZone = table.Column<int>(type: "int", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
