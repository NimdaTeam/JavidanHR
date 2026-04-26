using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayrollSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addMultipleInsuranceRatesToWorkshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsuranceRate",
                table: "Workshops");

            migrationBuilder.AddColumn<long>(
                name: "EmployeeInsuranceRate",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                defaultValue: 7L);

            migrationBuilder.AddColumn<long>(
                name: "EmployerInsuranceRate",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                defaultValue: 20L);

            migrationBuilder.AddColumn<long>(
                name: "UnEmploymentInsuranceRate",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                defaultValue: 3L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeInsuranceRate",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "EmployerInsuranceRate",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "UnEmploymentInsuranceRate",
                table: "Workshops");

            migrationBuilder.AddColumn<long>(
                name: "InsuranceRate",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
