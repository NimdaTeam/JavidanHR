using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayrollSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSystemCodeToPayItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomPayItemCodes");

            // Make PayItemId nullable
            migrationBuilder.AlterColumn<long>(
                name: "PayItemId",
                table: "ContractPayItems",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<string>(
                name: "SystemCode",
                table: "ContractPayItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // Add check constraint to ensure either PayItemId or SystemCode is set
            migrationBuilder.Sql(@"
            ALTER TABLE ContractPayItems 
            ADD CONSTRAINT CK_ContractPayItems_PayItemId_Or_SystemCode 
            CHECK ((PayItemId IS NOT NULL AND SystemCode IS NULL) OR (PayItemId IS NULL AND SystemCode IS NOT NULL))
        ");

            // Add index on SystemCode for performance
            migrationBuilder.CreateIndex(
                name: "IX_ContractPayItems_SystemCode",
                table: "ContractPayItems",
                column: "SystemCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContractPayItems_SystemCode",
                table: "ContractPayItems");

            migrationBuilder.Sql("ALTER TABLE ContractPayItems DROP CONSTRAINT CK_ContractPayItems_PayItemId_Or_SystemCode");

            migrationBuilder.DropColumn(
                name: "SystemCode",
                table: "ContractPayItems");

            migrationBuilder.AlterColumn<long>(
                name: "PayItemId",
                table: "ContractPayItems",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

        migrationBuilder.CreateTable(
                name: "CustomPayItemCodes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomPayItemCodes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomPayItemCodes_Code_IsDeleted",
                table: "CustomPayItemCodes",
                columns: new[] { "Code", "IsDeleted" },
                unique: true);
        }
    }
}
