using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateCaptureTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "urlImage",
                table: "Capture",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "urlImage",
                table: "Capture");
        }
    }
}
