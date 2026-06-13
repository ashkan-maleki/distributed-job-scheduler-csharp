using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Master.App.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkersState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkersStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DesiredNumberOfWorkers = table.Column<int>(type: "INTEGER", nullable: false),
                    NumberOfWorkersToRegister = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkersStates", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkersStates");
        }
    }
}
