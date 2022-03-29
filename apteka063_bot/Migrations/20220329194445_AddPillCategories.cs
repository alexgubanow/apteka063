using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apteka063.Migrations
{
    public partial class AddPillCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PillCategory",
                table: "Pills");

            migrationBuilder.AddColumn<string>(
                name: "PillCategoryName",
                table: "Pills",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PillCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PillCategories", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PillCategories");

            migrationBuilder.DropColumn(
                name: "PillCategoryName",
                table: "Pills");

            migrationBuilder.AddColumn<int>(
                name: "PillCategory",
                table: "Pills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
