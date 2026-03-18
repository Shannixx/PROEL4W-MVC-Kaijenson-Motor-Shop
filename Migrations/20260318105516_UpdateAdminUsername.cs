using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROEL4W_MVC_Kaijenson_Motor_Shop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Email",
                value: "admin123");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Email",
                value: "admin@kaijenson.com");
        }
    }
}
