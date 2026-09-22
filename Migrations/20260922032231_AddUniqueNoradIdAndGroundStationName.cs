using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbitWatch.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNoradIdAndGroundStationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Satellites_NoradId",
                table: "Satellites",
                column: "NoradId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroundStations_Name",
                table: "GroundStations",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Satellites_NoradId",
                table: "Satellites");

            migrationBuilder.DropIndex(
                name: "IX_GroundStations_Name",
                table: "GroundStations");
        }
    }
}
