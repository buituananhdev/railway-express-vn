using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainToTrainSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrainId",
                table: "TrainSchedules",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_TrainId",
                table: "TrainSchedules",
                column: "TrainId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainSchedules_Trains_TrainId",
                table: "TrainSchedules",
                column: "TrainId",
                principalTable: "Trains",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrainSchedules_Trains_TrainId",
                table: "TrainSchedules");

            migrationBuilder.DropIndex(
                name: "IX_TrainSchedules_TrainId",
                table: "TrainSchedules");

            migrationBuilder.DropColumn(
                name: "TrainId",
                table: "TrainSchedules");
        }
    }
}
