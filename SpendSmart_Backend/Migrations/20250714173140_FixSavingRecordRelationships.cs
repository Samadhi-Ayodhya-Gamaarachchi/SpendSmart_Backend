using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendSmart_Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixSavingRecordRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Users_UserId",
                table: "Goals");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Goals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Goals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SavingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GoalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingRecords_Goals_GoalId",
                        column: x => x.GoalId,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Goals_UserId1",
                table: "Goals",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_SavingRecords_GoalId",
                table: "SavingRecords",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingRecords_UserId",
                table: "SavingRecords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Users_UserId",
                table: "Goals",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Users_UserId1",
                table: "Goals",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Users_UserId",
                table: "Goals");

            migrationBuilder.DropForeignKey(
                name: "FK_Goals_Users_UserId1",
                table: "Goals");

            migrationBuilder.DropTable(
                name: "SavingRecords");

            migrationBuilder.DropIndex(
                name: "IX_Goals_UserId1",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Goals");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Goals_Users_UserId",
                table: "Goals",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
