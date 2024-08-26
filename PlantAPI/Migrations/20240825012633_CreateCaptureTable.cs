using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateCaptureTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Capture",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    deviceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isDesease = table.Column<bool>(type: "bit", nullable: false),
                    scientific_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deseasesResult = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capture", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Capture");
        }
    }
}
