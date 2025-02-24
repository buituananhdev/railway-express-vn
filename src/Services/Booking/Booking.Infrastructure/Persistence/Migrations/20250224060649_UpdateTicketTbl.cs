using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalStationId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "DepartureStationId",
                table: "Tickets",
                newName: "TrainScheduleId");

            migrationBuilder.AddColumn<DateTime>(
                name: "BookingDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "JourneyDate",
                table: "Tickets",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "JourneyDate",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "TrainScheduleId",
                table: "Tickets",
                newName: "DepartureStationId");

            migrationBuilder.AddColumn<Guid>(
                name: "ArrivalStationId",
                table: "Tickets",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }
    }
}
