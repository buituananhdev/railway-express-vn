using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePassenerInfoTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityType",
                table: "PassengerInfos");

            migrationBuilder.AddColumn<bool>(
                name: "IsMainPassenger",
                table: "PassengerInfos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMainPassenger",
                table: "PassengerInfos");

            migrationBuilder.AddColumn<int>(
                name: "IdentityType",
                table: "PassengerInfos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
