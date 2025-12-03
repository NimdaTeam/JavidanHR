using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationSystem.Migrations
{
    /// <inheritdoc />
    public partial class addCreatorIdToAutneticationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatorId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatorId",
                table: "UserRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatorId",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatorId",
                table: "RolePermissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatorId",
                table: "OtpCodes",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "OtpCodes");
        }
    }
}
