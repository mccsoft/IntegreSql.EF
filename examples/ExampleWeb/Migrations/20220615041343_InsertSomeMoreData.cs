using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleWeb.Migrations
{
    public partial class InsertSomeMoreData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name" },
                values: new object[,] { { 3, "Ilon" }, }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
