using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbitWatch.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryAndAgencyRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "Satellites");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "Satellites");

            migrationBuilder.DropColumn(
                name: "Agency",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "GroundStations");

            migrationBuilder.DropColumn(
                name: "Operator",
                table: "GroundStations");

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "Satellites",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Satellites",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "Missions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "AgencyId",
                table: "GroundStations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "GroundStations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "India" },
                    { 2, "United States" },
                    { 3, "Germany" },
                    { 4, "France" },
                    { 5, "Japan" }
                });

                        migrationBuilder.InsertData(
                            table: "Agencies",
                            columns: new[] { "Id", "Name" },
                            values: new object[,]
                            {
                    { 1, "ISRO" },
                    { 2, "NASA" },
                    { 3, "ESA" },
                    { 4, "JAXA" },
                    { 5, "DLR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Satellites_AgencyId",
                table: "Satellites",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Satellites_CountryId",
                table: "Satellites",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_AgencyId",
                table: "Missions",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_GroundStations_AgencyId",
                table: "GroundStations",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_GroundStations_CountryId",
                table: "GroundStations",
                column: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroundStations_Agencies_AgencyId",
                table: "GroundStations",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroundStations_Countries_CountryId",
                table: "GroundStations",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Agencies_AgencyId",
                table: "Missions",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Satellites_Agencies_AgencyId",
                table: "Satellites",
                column: "AgencyId",
                principalTable: "Agencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Satellites_Countries_CountryId",
                table: "Satellites",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroundStations_Agencies_AgencyId",
                table: "GroundStations");

            migrationBuilder.DropForeignKey(
                name: "FK_GroundStations_Countries_CountryId",
                table: "GroundStations");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Agencies_AgencyId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Satellites_Agencies_AgencyId",
                table: "Satellites");

            migrationBuilder.DropForeignKey(
                name: "FK_Satellites_Countries_CountryId",
                table: "Satellites");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Satellites_AgencyId",
                table: "Satellites");

            migrationBuilder.DropIndex(
                name: "IX_Satellites_CountryId",
                table: "Satellites");

            migrationBuilder.DropIndex(
                name: "IX_Missions_AgencyId",
                table: "Missions");

            migrationBuilder.DropIndex(
                name: "IX_GroundStations_AgencyId",
                table: "GroundStations");

            migrationBuilder.DropIndex(
                name: "IX_GroundStations_CountryId",
                table: "GroundStations");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Satellites");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Satellites");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "GroundStations");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "GroundStations");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Satellites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "Satellites",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Agency",
                table: "Missions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "GroundStations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                table: "GroundStations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
