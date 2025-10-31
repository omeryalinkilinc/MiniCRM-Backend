using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniCRM.Api.Migrations
{
    public partial class AddPhotoUrlToCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Customers");
        }
    }
}
