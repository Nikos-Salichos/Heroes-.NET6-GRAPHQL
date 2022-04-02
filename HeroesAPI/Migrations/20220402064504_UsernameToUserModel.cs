using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeroesAPI.Migrations
{
    public partial class UsernameToUserModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsEmailConfirmed",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");
        }
    }
}
