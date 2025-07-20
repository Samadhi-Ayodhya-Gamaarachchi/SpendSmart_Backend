using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendSmart_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToAdminTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Admins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpiry",
                table: "Admins",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpiry",
                table: "Admins");
        }
    }
}
