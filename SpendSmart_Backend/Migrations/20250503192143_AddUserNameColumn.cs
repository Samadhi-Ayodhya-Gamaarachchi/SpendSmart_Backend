using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendSmart_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Admins",
                newName: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Admins",
                newName: "Name");
        }
    }
}
