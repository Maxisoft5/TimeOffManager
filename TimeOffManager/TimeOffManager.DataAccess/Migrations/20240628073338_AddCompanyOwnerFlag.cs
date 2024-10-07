using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeOffManager.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyOwnerFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsExecutive",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsExecutive",
                table: "AspNetUsers");
        }
    }
}
