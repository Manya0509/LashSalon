using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LashBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudioNameAndHeroPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeroPhotoFileName",
                table: "AboutInfos",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudioName",
                table: "AboutInfos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroPhotoFileName",
                table: "AboutInfos");

            migrationBuilder.DropColumn(
                name: "StudioName",
                table: "AboutInfos");
        }
    }
}
