using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class InitialDbCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSettings",
                columns: table => new
                {
                    GameSettingsId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BoardHeight = table.Column<int>(nullable: false),
                    BoardWidth = table.Column<int>(nullable: false),
                    IsHumanVsComputer = table.Column<bool>(nullable: false),
                    WinningCondition = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSettings", x => x.GameSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "SavedGames",
                columns: table => new
                {
                    SavedGameId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameName = table.Column<string>(maxLength: 32, nullable: false),
                    GameBoard = table.Column<string>(nullable: false),
                    PlayerOneMoves = table.Column<bool>(nullable: false),
                    IsHumanVsComputer = table.Column<bool>(nullable: false),
                    PlayerOneName = table.Column<string>(nullable: false),
                    PlayerTwoName = table.Column<string>(nullable: false),
                    WinningCondition = table.Column<int>(nullable: false),
                    GameIsOver = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedGames", x => x.SavedGameId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSettings");

            migrationBuilder.DropTable(
                name: "SavedGames");
        }
    }
}
