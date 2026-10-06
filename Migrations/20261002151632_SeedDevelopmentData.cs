using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbitWatch.Migrations
{
    /// <inheritdoc />
    public partial class SeedDevelopmentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Missions",
                columns: new[] { "Id", "AgencyId", "Description", "EndDate", "LaunchDate", "Name", "Status" },
                values: new object[] { 1, 2, null, null, new DateTime(1998, 11, 20, 0, 0, 0, 0, DateTimeKind.Utc), "ISS Operations", "Active" });

            migrationBuilder.InsertData(
                table: "Satellites",
                columns: new[] { "Id", "AgencyId", "CountryId", "LaunchDate", "MissionId", "Name", "NoradId", "OrbitType", "Status" },
                values: new object[] { 1, 2, 2, new DateTime(1998, 11, 20, 0, 0, 0, 0, DateTimeKind.Utc), 1, "ISS (ZARYA)", "25544", "LEO", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Satellites",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Missions",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
