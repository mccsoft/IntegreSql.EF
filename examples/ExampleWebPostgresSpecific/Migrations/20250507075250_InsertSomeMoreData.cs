using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleWebPostgresSpecific.Migrations
{
    /// <inheritdoc />
    public partial class InsertSomeMoreData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 3, "Ilon" },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
