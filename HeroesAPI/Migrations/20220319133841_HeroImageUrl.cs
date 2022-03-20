using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeroesAPI.Migrations
{
    public partial class HeroImageUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Heroes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Heroes");
        }
    }
}
