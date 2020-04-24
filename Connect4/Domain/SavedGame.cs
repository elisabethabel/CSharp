using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class SavedGame
    {
        public int SavedGameId { get; set; }
        [Display(Name = "Game name")] [MinLength(1)] [MaxLength(32)] 
        public string GameName { get; set; } = default!;
        public string GameBoard { get; set; } = default!;
        
        public bool PlayerOneMoves { get; set; }
        public bool IsHumanVsComputer { get; set; }
        public string PlayerOneName { get; set; } = default!;
        public string PlayerTwoName { get; set; } = default!;
        public int WinningCondition { get; set; } = 4;
        public bool GameIsOver { get; set; }
        public string Message { get; set; } = "";
    }
}