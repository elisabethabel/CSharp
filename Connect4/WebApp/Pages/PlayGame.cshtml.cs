using System;
using System.Threading.Tasks;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.PlayGame
{
    public class PlayGameModel : PageModel
    {
        private readonly DAL.AppDbContext _context;
        
        public readonly GameEngine.Game Game;
        
        public PlayGameModel(AppDbContext context)
        {
            _context = context;
            Game = new GameEngine.Game(_context);
        }
        
        public async Task<ActionResult> OnGet(int? gameId, int? col, int? row, bool? computerMove)
        {
            if (gameId == null)
            {
                return RedirectToPage("/NewGame");
            }
            
            var savedGame = Game.RestoreSavedGameFromDb(gameId.Value);

            if (savedGame.IsHumanVsComputer)
            {
                savedGame.PlayerTwoName = "Computer";
            }
            if (computerMove != null)
            {
                col = Game.ComputerMove(savedGame.WinningCondition);
                row = Game.FindYIndex(col.Value);
            }
            
            if (col != null && row != null)
            {
                Game.Move(row.Value, col.Value);
                CellState playerState = Game.PlayerOneMoves ? CellState.G : CellState.R;
                if (Game.IsWinning(row.Value, col.Value, savedGame.WinningCondition, playerState ))
                {
                    var player = Game.PlayerOneMoves ? Game.PlayerTwoName : Game.PlayerOneName;
                    Game.Message = player + " WON!";
                    Game.GameIsOver = true;
                }
                else if (Game.TableIsFull())
                {
                    Game.Message = "IT'S A TIE'!";
                    Game.GameIsOver = true;
                }
                Game.SaveGameToDb(savedGame.IsHumanVsComputer, savedGame.WinningCondition);
                await _context.SaveChangesAsync();
            }

            if (savedGame.IsHumanVsComputer && !Game.PlayerOneMoves)
            {
                return RedirectToPage("/PlayGame", new {gameId = savedGame.SavedGameId, computerMove=true});
            }
            return Page();
        }
    }
}