using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Master.App.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyTokenToJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Version",
                table: "Workers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldRowVersion: true);

            migrationBuilder.AlterColumn<long>(
                name: "Version",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldRowVersion: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Version",
                table: "Workers",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Version",
                table: "Jobs",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");
        }
    }
}
