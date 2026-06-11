using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Master.App.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddDesiredState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HeartBeat",
                table: "Workers",
                newName: "RegisteredAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastHeartBeat",
                table: "Workers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "SchedulerStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DesiredNumberOfWorkers = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerStates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchedulerStates");

            migrationBuilder.DropColumn(
                name: "LastHeartBeat",
                table: "Workers");

            migrationBuilder.RenameColumn(
                name: "RegisteredAt",
                table: "Workers",
                newName: "HeartBeat");
        }
    }
}
