using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendSmart_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailChangeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add the new email change tracking columns to Users table
            migrationBuilder.AddColumn<string>(
                name: "EmailChangeToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailChangeTokenExpiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingEmail",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailChangeToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailChangeTokenExpiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PendingEmail",
                table: "Users");
        }
    }
}
