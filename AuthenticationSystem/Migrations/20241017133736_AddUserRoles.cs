using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OtTPCodes_Users_UserId",
                table: "OtTPCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OtTPCodes",
                table: "OtTPCodes");

            migrationBuilder.RenameTable(
                name: "OtTPCodes",
                newName: "OtpCodes");

            migrationBuilder.RenameIndex(
                name: "IX_OtTPCodes_UserId",
                table: "OtpCodes",
                newName: "IX_OTPCodes_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OTPCodes",
                table: "OtpCodes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OTPCodes_Users_UserId",
                table: "OtpCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OTPCodes_Users_UserId",
                table: "OtpCodes");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OTPCodes",
                table: "OtpCodes");

            migrationBuilder.RenameTable(
                name: "OtpCodes",
                newName: "OtTPCodes");

            migrationBuilder.RenameIndex(
                name: "IX_OTPCodes_UserId",
                table: "OtTPCodes",
                newName: "IX_OtTPCodes_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OtTPCodes",
                table: "OtTPCodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OtTPCodes_Users_UserId",
                table: "OtTPCodes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
