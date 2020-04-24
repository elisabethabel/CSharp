using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class GameSettings
    {
        public int GameSettingsId { get; set; }
        [Display(Name = "Board Height")] [Range(4, 30, ErrorMessage = "Keep {0} in range of {1} to {2}")]
        public int BoardHeight { get; set; } = 6;
        [Display(Name = "Board Width")] [Range(5, 30, ErrorMessage = "Keep {0} in range of {1} to {2}")]
        public int BoardWidth { get; set; } = 7;
        [Display(Name = "Play Against Computer")]
        public bool IsHumanVsComputer { get; set; }
        [Display(Name = "Winning Condition")]
        public int WinningCondition { get; set; } = 4;
    }
}