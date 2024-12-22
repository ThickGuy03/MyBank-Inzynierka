using Microsoft.EntityFrameworkCore.Migrations;

namespace Inzynierka.Migrations
{
    public partial class IdToTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete any existing roles with these IDs to avoid conflicts
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3bacfc1f-5ec2-4aee-b0e5-aeebbe550394");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8efbbc51-bf7c-4cd9-adbe-35fc33809781");

            // Alter the 'Note' column to fix the data type
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "navchar(100)",
                oldNullable: true);

            // Change 'Amount' column to decimal type
            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Add 'UserId' column to Transactions
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Modify 'Categories' table columns
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Categories",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "navchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Categories",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "navchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "nvarchar(5)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "navchar(5)");

            // Check if the roles already exist, and insert them only if they don't
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'client')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName)
                    VALUES ('1b2b381b-7435-46cb-a0f2-3e20a30a02ae', 'client', 'client')
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'admin')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName)
                    VALUES ('c815988a-5176-48ff-97e1-9e5044faf30d', 'admin', 'admin')
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete the inserted roles
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1b2b381b-7435-46cb-a0f2-3e20a30a02ae");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c815988a-5176-48ff-97e1-9e5044faf30d");

            // Drop the 'UserId' column from Transactions
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            // Revert the changes to the columns in the 'Transactions' table
            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "Transactions",
                type: "navchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Amount",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            // Revert the changes to the columns in the 'Categories' table
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Categories",
                type: "navchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Categories",
                type: "navchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "navchar(5)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(5)");

            // Reinsert the previous roles
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3bacfc1f-5ec2-4aee-b0e5-aeebbe550394", null, "client", "client" },
                    { "8efbbc51-bf7c-4cd9-adbe-35fc33809781", null, "admin", "admin" }
                });
        }
    }
}
