using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStationInfomation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "Stations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "KilometricPoint",
                table: "Stations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityName",
                table: "Stations");

            migrationBuilder.DropColumn(
                name: "KilometricPoint",
                table: "Stations");
        }
    }
}
