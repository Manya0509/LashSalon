using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LashBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Clients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Clients");
        }
    }
}
