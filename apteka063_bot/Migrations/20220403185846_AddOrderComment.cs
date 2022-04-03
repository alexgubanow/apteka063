using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apteka063.Migrations
{
    public partial class AddOrderComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderComment",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderComment",
                table: "Orders");
        }
    }
}
