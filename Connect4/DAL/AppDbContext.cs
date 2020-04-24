using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class AppDbContext : DbContext
    {
        public DbSet<GameSettings> GameSettings { get; set; } = default!;
        public DbSet<SavedGame> SavedGames { get; set; } = default!;
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}