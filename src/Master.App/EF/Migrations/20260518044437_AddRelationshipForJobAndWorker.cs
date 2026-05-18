using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Master.App.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationshipForJobAndWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Workers",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "Jobs",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_WorkerId",
                table: "Jobs",
                column: "WorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Workers_WorkerId",
                table: "Jobs",
                column: "WorkerId",
                principalTable: "Workers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Workers_WorkerId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_WorkerId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Jobs");
        }
    }
}
