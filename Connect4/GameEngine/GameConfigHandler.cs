using System.Linq;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace GameEngine
{
    public static class GameConfigHandler
    {
        private static DbContextOptions options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite
            (@"Data source=C:\Users\admin\OneDrive\Dokumendid\icd0008\icd0008-2019f\Connect4\WebApp\connect4Db.db")
            .Options;
        public static void SaveConfig(GameSettings gameSettings)
        {
            using (var ctx = new AppDbContext(options))
            {
                ctx.GameSettings.Add(gameSettings);
                ctx.SaveChanges();
            }
        }

        public static GameSettings LoadConfig()
        {
            GameSettings lastGameSettings;
            using (var ctx = new AppDbContext(options))
            {
                lastGameSettings = ctx.GameSettings.FirstOrDefault(g =>
                    g.GameSettingsId == ctx.GameSettings.Max(x => x.GameSettingsId));
            }

            if (lastGameSettings == null)
            {
                lastGameSettings = new GameSettings()
                {
                    BoardHeight = 6,
                    BoardWidth = 7
                };
            }

            return lastGameSettings;
        }
    }
}