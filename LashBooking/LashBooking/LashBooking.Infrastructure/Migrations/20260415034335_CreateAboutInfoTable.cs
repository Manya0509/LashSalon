using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LashBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAboutInfoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AboutInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AboutText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EducationText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkingHours = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WhatsAppLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TelegramLink = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PhotoFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutInfos");
        }
    }
}
