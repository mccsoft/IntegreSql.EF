using ExampleWebPostgresSpecific.Database;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleWebPostgresSpecific.Migrations
{
    /// <inheritdoc />
    public partial class EnumSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:user_type", "admin,normal");

            migrationBuilder.AddColumn<UserType>(
                name: "UserType",
                table: "Users",
                type: "user_type",
                nullable: false,
                defaultValue: UserType.Normal);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "UserType",
                value: UserType.Normal);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "UserType",
                value: UserType.Admin);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "Users");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:user_type", "admin,normal");
        }
    }
}
