using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelListing.Api.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "9ecb7124-41f1-4452-b7d9-621f841d34bf", "0dfcacd1-37d1-4672-b10c-dc6559b733c9", "User", "USER" },
                    { "c1c8d006-3fc7-425d-be50-6742be8c16ff", "6b1c4971-54bc-4f0b-9565-89fe04466c20", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9ecb7124-41f1-4452-b7d9-621f841d34bf");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c1c8d006-3fc7-425d-be50-6742be8c16ff");
        }
    }
}
