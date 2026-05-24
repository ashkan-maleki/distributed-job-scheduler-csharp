using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Master.App.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddHeartBeatToWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentState",
                table: "Workers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "HeartBeat",
                table: "Workers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Workers_Name",
                table: "Workers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workers_Name",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "CurrentState",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "HeartBeat",
                table: "Workers");
        }
    }
}
