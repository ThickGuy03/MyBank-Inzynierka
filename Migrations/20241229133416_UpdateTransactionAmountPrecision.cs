using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Inzynierka.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionAmountPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
            name: "Amount",
            table: "Transactions",
            type: "decimal(18,2)",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            {
                migrationBuilder.AlterColumn<decimal>(
                    name: "Amount",
                    table: "Transactions",
                    type: "decimal(18,0)",
                    nullable: false,
                    oldClrType: typeof(decimal),
                    oldType: "decimal(18,2)");
            }
        }
    }
}
