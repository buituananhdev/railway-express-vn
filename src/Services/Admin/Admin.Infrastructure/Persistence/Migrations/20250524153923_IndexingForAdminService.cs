using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IndexingForAdminService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TrainName",
                table: "Trains",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "StationName",
                table: "Stations",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CityName",
                table: "Stations",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TrainStatuses_StationId_Status",
                table: "TrainStatuses",
                columns: new[] { "StationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainStatuses_Status",
                table: "TrainStatuses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TrainStatuses_TrainId_StationId",
                table: "TrainStatuses",
                columns: new[] { "TrainId", "StationId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_ArrivalStationId_ArrivalTime",
                table: "TrainSchedules",
                columns: new[] { "ArrivalStationId", "ArrivalTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_ArrivalTime",
                table: "TrainSchedules",
                column: "ArrivalTime");

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_DepartureStationId_ArrivalStationId",
                table: "TrainSchedules",
                columns: new[] { "DepartureStationId", "ArrivalStationId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_DepartureStationId_DepartureTime_IsReturnTrip",
                table: "TrainSchedules",
                columns: new[] { "DepartureStationId", "DepartureTime", "IsReturnTrip" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_DepartureTime",
                table: "TrainSchedules",
                column: "DepartureTime");

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_IsReturnTrip",
                table: "TrainSchedules",
                column: "IsReturnTrip");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_Track",
                table: "Trains",
                column: "Track");

            migrationBuilder.CreateIndex(
                name: "IX_Trains_TrainName",
                table: "Trains",
                column: "TrainName");

            migrationBuilder.CreateIndex(
                name: "IX_TrainCars_SeatType",
                table: "TrainCars",
                column: "SeatType");

            migrationBuilder.CreateIndex(
                name: "IX_TrainCars_TrainId_CarNumber",
                table: "TrainCars",
                columns: new[] { "TrainId", "CarNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Stations_CityName",
                table: "Stations",
                column: "CityName");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_CityName_StationName",
                table: "Stations",
                columns: new[] { "CityName", "StationName" });

            migrationBuilder.CreateIndex(
                name: "IX_Stations_StationName",
                table: "Stations",
                column: "StationName");

            migrationBuilder.CreateIndex(
                name: "IX_Stations_StationOrder",
                table: "Stations",
                column: "StationOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_TrainCarId_SeatNumber",
                table: "Seats",
                columns: new[] { "TrainCarId", "SeatNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainStatuses_StationId_Status",
                table: "TrainStatuses");

            migrationBuilder.DropIndex(
                name: "IX_TrainStatuses_Status",
                table: "TrainStatuses");

            migrationBuilder.DropIndex(
                name: "IX_TrainStatuses_TrainId_StationId",
                table: "TrainStatuses");

            migrationBuilder.DropIndex(
                name: "IX_TrainSchedules_ArrivalStationId_ArrivalTime",
                table: "TrainSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainSchedules_ArrivalTime",
                table: "TrainSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainSchedules_DepartureStationId_ArrivalStationId",
                table: "TrainSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainSchedules_DepartureStationId_DepartureTime_IsReturnTrip",
                table: "TrainSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainSchedules_DepartureTime",
                table: "TrainSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainSchedules_IsReturnTrip",
                table: "TrainSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Trains_Track",
                table: "Trains");

            migrationBuilder.DropIndex(
                name: "IX_Trains_TrainName",
                table: "Trains");

            migrationBuilder.DropIndex(
                name: "IX_TrainCars_SeatType",
                table: "TrainCars");

            migrationBuilder.DropIndex(
                name: "IX_TrainCars_TrainId_CarNumber",
                table: "TrainCars");

            migrationBuilder.DropIndex(
                name: "IX_Stations_CityName",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_CityName_StationName",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_StationName",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Stations_StationOrder",
                table: "Stations");

            migrationBuilder.DropIndex(
                name: "IX_Seats_TrainCarId_SeatNumber",
                table: "Seats");

            migrationBuilder.AlterColumn<string>(
                name: "TrainName",
                table: "Trains",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "StationName",
                table: "Stations",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CityName",
                table: "Stations",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
