using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeroesAPI.Migrations
{
    public partial class deletefl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Lastname",
                table: "Users",
                newName: "Username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "Lastname");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
