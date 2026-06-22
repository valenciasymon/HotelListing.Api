using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelListing.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNameHotelAdmins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins");

            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Hotels_HotelId",
                table: "Admins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Admins",
                table: "Admins");

            migrationBuilder.RenameTable(
                name: "Admins",
                newName: "HotelAdmins");

            migrationBuilder.RenameIndex(
                name: "IX_Admins_HotelId",
                table: "HotelAdmins",
                newName: "IX_HotelAdmins_HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Admins_ApplicationUserId",
                table: "HotelAdmins",
                newName: "IX_HotelAdmins_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HotelAdmins",
                table: "HotelAdmins",
                column: "HotelAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_HotelAdmins_AspNetUsers_ApplicationUserId",
                table: "HotelAdmins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HotelAdmins_Hotels_HotelId",
                table: "HotelAdmins",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HotelAdmins_AspNetUsers_ApplicationUserId",
                table: "HotelAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_HotelAdmins_Hotels_HotelId",
                table: "HotelAdmins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HotelAdmins",
                table: "HotelAdmins");

            migrationBuilder.RenameTable(
                name: "HotelAdmins",
                newName: "Admins");

            migrationBuilder.RenameIndex(
                name: "IX_HotelAdmins_HotelId",
                table: "Admins",
                newName: "IX_Admins_HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_HotelAdmins_ApplicationUserId",
                table: "Admins",
                newName: "IX_Admins_ApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Admins",
                table: "Admins",
                column: "HotelAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_AspNetUsers_ApplicationUserId",
                table: "Admins",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Hotels_HotelId",
                table: "Admins",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
