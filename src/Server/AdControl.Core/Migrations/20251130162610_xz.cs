using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdControl.Core.Migrations
{
    /// <inheritdoc />
    public partial class xz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestCICD",
                table: "Configs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestCICD",
                table: "Configs");
        }
    }
}
