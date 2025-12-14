using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdControl.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_screens_count_field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScreensCount",
                table: "Configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScreensCount",
                table: "Configs");
        }
    }
}
