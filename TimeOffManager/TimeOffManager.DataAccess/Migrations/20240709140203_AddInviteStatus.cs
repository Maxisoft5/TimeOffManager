using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeOffManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InviteStatus",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteStatus",
                table: "AspNetUsers");
        }
    }
}
