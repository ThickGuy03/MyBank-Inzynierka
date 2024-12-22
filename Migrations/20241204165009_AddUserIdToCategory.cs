using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inzynierka.Migrations
{
    public partial class AddUserIdToCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7dc48412-0f31-4df8-a013-4779b14c94af");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "974c3897-38a0-4354-a497-64302e2de1d9");

            // Add UserId to Categories
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Categories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId",
                table: "Categories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_UserId",
                table: "Categories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Insert roles only if they don't exist
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'client') " +
                                  "BEGIN INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES ('3bacfc1f-5ec2-4aee-b0e5-aeebbe550394', 'client', 'CLIENT') END");

            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'admin') " +
                                  "BEGIN INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES ('8efbbc51-bf7c-4cd9-adbe-35fc33809781', 'admin', 'ADMIN') END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_UserId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UserId",
                table: "Categories");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3bacfc1f-5ec2-4aee-b0e5-aeebbe550394");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8efbbc51-bf7c-4cd9-adbe-35fc33809781");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Categories");

            // Re-insert roles in case of rollback (in case this data is needed)
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "7dc48412-0f31-4df8-a013-4779b14c94af", null, "client", "client" },
                    { "974c3897-38a0-4354-a497-64302e2de1d9", null, "admin", "admin" }
                });
        }
    }
}
