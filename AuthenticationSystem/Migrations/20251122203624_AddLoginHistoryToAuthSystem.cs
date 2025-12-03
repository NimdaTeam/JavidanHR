using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddLoginHistoryToAuthSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLoginHistory_Users_UserId",
                table: "UserLoginHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLoginHistory",
                table: "UserLoginHistory");

            migrationBuilder.RenameTable(
                name: "UserLoginHistory",
                newName: "UserLoginHistories");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistory_UserId_IsSuccessful",
                table: "UserLoginHistories",
                newName: "IX_UserLoginHistories_UserId_IsSuccessful");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistory_UserId",
                table: "UserLoginHistories",
                newName: "IX_UserLoginHistories_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistory_LoginAt",
                table: "UserLoginHistories",
                newName: "IX_UserLoginHistories_LoginAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistory_IpAddress",
                table: "UserLoginHistories",
                newName: "IX_UserLoginHistories_IpAddress");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLoginHistories",
                table: "UserLoginHistories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLoginHistories_Users_UserId",
                table: "UserLoginHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLoginHistories_Users_UserId",
                table: "UserLoginHistories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLoginHistories",
                table: "UserLoginHistories");

            migrationBuilder.RenameTable(
                name: "UserLoginHistories",
                newName: "UserLoginHistory");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistories_UserId_IsSuccessful",
                table: "UserLoginHistory",
                newName: "IX_UserLoginHistory_UserId_IsSuccessful");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistories_UserId",
                table: "UserLoginHistory",
                newName: "IX_UserLoginHistory_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistories_LoginAt",
                table: "UserLoginHistory",
                newName: "IX_UserLoginHistory_LoginAt");

            migrationBuilder.RenameIndex(
                name: "IX_UserLoginHistories_IpAddress",
                table: "UserLoginHistory",
                newName: "IX_UserLoginHistory_IpAddress");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLoginHistory",
                table: "UserLoginHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLoginHistory_Users_UserId",
                table: "UserLoginHistory",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
