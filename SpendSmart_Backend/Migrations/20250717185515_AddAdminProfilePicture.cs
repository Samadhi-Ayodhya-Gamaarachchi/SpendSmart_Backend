using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendSmart_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminProfilePicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicture",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureFileName",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfilePictureUploadedAt",
                table: "Admins",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ProfilePictureFileName",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUploadedAt",
                table: "Admins");
        }
    }
}
