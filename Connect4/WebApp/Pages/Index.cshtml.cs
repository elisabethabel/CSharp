using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public IndexModel(DAL.AppDbContext context)
        {
            _context = context;
        }
        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty] public GameSettings GameSettings { get; set; } = default!;

        [BindProperty] public Game Game { get; set; } = default!;
        
        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var maxWinningCondition = 0;
                if (GameSettings.BoardHeight >= GameSettings.BoardWidth)
                {
                    maxWinningCondition = GameSettings.BoardHeight;
                }
                else
                {
                    maxWinningCondition = GameSettings.BoardWidth;
                }

                if (GameSettings.WinningCondition > maxWinningCondition)
                {
                    ModelState.AddModelError(nameof(GameSettings.WinningCondition), "Too many chips in a row to win");
                    return Page();
                }
                Game.InitializeNewGame(GameSettings.BoardHeight, GameSettings.BoardWidth);
                var savedGame = new SavedGame()
                {
                    GameName = Game.GameName,
                    GameBoard = Game.SerializeBoard(),
                    PlayerOneMoves = Game.PlayerOneMoves,
                    IsHumanVsComputer = GameSettings.IsHumanVsComputer,
                    PlayerOneName = Game.PlayerOneName,
                    PlayerTwoName = Game.PlayerTwoName,
                    WinningCondition = GameSettings.WinningCondition
                };
                _context.SavedGames.Add(savedGame);
                await _context.SaveChangesAsync();
                return RedirectToPage("/PlayGame", new {gameId = savedGame.SavedGameId});
            }
            return Page();
        }
    }
}