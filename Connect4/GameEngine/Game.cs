using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DAL;
using Domain;
using Newtonsoft.Json;

namespace GameEngine
{
    /// <summary>
    /// Connect 4
    /// </summary>
    public class Game
    {
        public int GameId { get; set; }
        private CellState[,] Board { get; set; } = default!;
        [MinLength(1)] [MaxLength(32)] [Display(Name = "Game name")]
        public string GameName { get; set; } = default!;
        
        public int BoardWidth { get; private set; }
        
        public int BoardHeight { get; private set; }

        [Display(Name = "Player One Name")]
        [MinLength(1)]
        [MaxLength(32)]
        public string PlayerOneName { get; set; } = "Player 1";

        [Display(Name = "Player Two Name")]
        [MinLength(1)]
        [MaxLength(32)]
        public string PlayerTwoName { get; set; } = "Player 2";
        
        [Display(Name = "Player One Starts")]
        public bool PlayerOneMoves { get; set; }

        public bool GameIsOver { get; set; }
        public string Message { get; set; } = "";
        
        private readonly AppDbContext? _ctx;
        

        public Game(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public Game()
        {
        }

        public CellState GetBoardCellValue(int y, int x)
        {
            return Board[y, x];
        }
        
        public void InitializeNewGame(int height, int width)
        {
            BoardHeight = height;
            BoardWidth = width;
            Board = new CellState[BoardHeight, BoardWidth];
        }
        
        public SavedGame RestoreSavedGameFromDb(int gameId)
        {
            var savedGame = _ctx!.SavedGames.First(s => s.SavedGameId == gameId);
            Board = DeSerializeBoard(savedGame.GameBoard);
            BoardHeight = Board.GetLength(0);
            BoardWidth = Board.GetLength(1);
            GameName = savedGame.GameName;
            Message = savedGame.Message;
            GameIsOver = savedGame.GameIsOver;
            GameId = savedGame.SavedGameId;
            PlayerOneName = savedGame.PlayerOneName;
            PlayerTwoName = savedGame.PlayerTwoName;
            PlayerOneMoves = savedGame.PlayerOneMoves;
            return savedGame;
        }

        public void SaveGameToDb(bool isHumanVsComputer, int winningCondition)
        { 
            if (!_ctx!.SavedGames.Any(g => g.SavedGameId == GameId))
            { 
                SavedGame savedGame = new SavedGame() 
                {
                    GameName = GameName,
                    GameBoard = SerializeBoard(),
                    PlayerOneMoves = PlayerOneMoves,
                    IsHumanVsComputer = isHumanVsComputer,
                    WinningCondition = winningCondition,
                    Message = Message,
                    GameIsOver = GameIsOver,
                    PlayerOneName = PlayerOneName,
                    PlayerTwoName = PlayerTwoName
                };
                _ctx.SavedGames.Add(savedGame);
                _ctx.SaveChanges();
                GameId = savedGame.SavedGameId;
            }
            else
            {
                var savedGame = _ctx.SavedGames.First(s => s.SavedGameId == GameId);
                Console.WriteLine(savedGame.SavedGameId);
                Console.WriteLine(GameId);
                savedGame.GameBoard = SerializeBoard();
                savedGame.PlayerOneMoves = PlayerOneMoves;
                savedGame.GameIsOver = GameIsOver;
                savedGame.Message = Message;
                _ctx.SavedGames.Update(savedGame);
                _ctx.SaveChanges();
            }
        }

        public CellState[,] GetBoard()
        {
            var result = new CellState[BoardHeight, BoardWidth];
            Array.Copy(Board, result, Board.Length);
            return result;
        }

        public void Move(int posY, int posX)
        {
            Board[posY, posX] = PlayerOneMoves ? CellState.R : CellState.G;
            PlayerOneMoves = !PlayerOneMoves;
        }

        public int ComputerMove(int winningCondition)
        {
            int posX;
            for (int xIndex = 0; xIndex < BoardWidth; xIndex++)
            {
                var yIndex = FindYIndex(xIndex);
                if (yIndex != -1)
                {
                    if (IsWinning(yIndex, xIndex, winningCondition, CellState.G) || IsWinning(yIndex, xIndex , winningCondition, CellState.R))
                    {
                        return xIndex;
                    }
                }
            }
            Random r = new Random();
            var correctX = false;
            do
            {
                posX = r.Next(0, BoardWidth);
                if (Board[0, posX] == CellState.Empty)
                {
                    correctX = true;
                }
            } while (!correctX);
            return posX;
        }

        public int FindYIndex(int posX)
        {
            if (posX >= 0)
            {
                for (int yIndex = BoardHeight - 1; yIndex > -1; yIndex--)
                {
                    if (Board[yIndex, posX] == CellState.Empty)
                    {
                        return yIndex;
                    }
                }
            }
            return -1;
        }
        public bool IsWinning(int yIndex, int xIndex, int winningCondition, CellState state)
        { 
            //checking vertically
            var count = 1;
            for (int y = yIndex + 1; y < BoardHeight; y++)
            {
                if (Board[y, xIndex] == state)
                {
                    count++;
                    if (count == winningCondition)
                    {
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }
            
            //checking horizontally
            count = 0;
            bool winPos = false;
            for (int x = 0; x < BoardWidth; x++)
            {
                if (Board[yIndex, x] == state || x == xIndex)
                {
                    if (x == xIndex)
                    {
                        winPos = true;
                    }
                    count++;
                    if (count == winningCondition && winPos)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 0;
                    winPos = false;
                }
            }
            
            //checking diagonal from down left to up right
            (int yPos, int xPos) = FindStartPos(yIndex, xIndex, true);
            var allChecked = false;
            count = 0;
            winPos = false;
            while (!allChecked) 
            {
                if (Board[yPos, xPos] == state || xPos == xIndex)
                {
                    if (xPos == xIndex)
                    {
                        winPos = true;
                    }
                    count++;
                    if (count == winningCondition && winPos)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 0;
                    winPos = false;
                }
                if (yPos - 1 > -1 && xPos + 1 < BoardWidth)
                {
                    yPos -= 1;
                    xPos += 1;
                }
                else
                {
                    allChecked = true;
                }
            }
            
            //checking diagonally from up right to down left
            (yPos, xPos) = FindStartPos(yIndex, xIndex, false);
            allChecked = false;
            count = 0;
            while (!allChecked)
            {
                if (Board[yPos,xPos] == state || xPos == xIndex)
                {
                    if (xPos == xIndex)
                    {
                        winPos = true;
                    }
                    count++;
                    if (count == winningCondition && winPos)
                    {
                        return true;
                    }
                }
                else
                {
                    count = 0;
                    winPos = false;
                }
                if (yPos - 1 > -1 && xPos - 1 > -1)
                {
                    yPos -= 1;
                    xPos -= 1;
                }
                else
                {
                    allChecked = true;
                }
            }

            return false;
        }

        private (int, int) FindStartPos(int yIndex, int xIndex, bool leftToRight)
        {
            int yPos = yIndex;
            int xPos = xIndex;
            bool beginningFound = false;
            while (!beginningFound) 
            {
                if (leftToRight)
                {
                    if (yPos == BoardHeight - 1 || xPos == 0)
                    {
                        beginningFound = true;
                    }
                    else
                    {
                        yPos += 1;
                        xPos -= 1;
                    }
                }
                else
                {
                    if (yPos == BoardHeight - 1 || xPos == BoardWidth - 1)
                    {
                        beginningFound = true;
                    }
                    else
                    {
                        yPos += 1;
                        xPos += 1;
                    } 
                }
            }
            return (yPos, xPos);
        } 

        public bool TableIsFull()
        {
            for (int x = 0; x < BoardWidth; x++)
            {
                if (Board[0, x] == CellState.Empty)
                {
                    return false;
                }
            }

            return true;
        }


        public string SerializeBoard()
        {
            return JsonConvert.SerializeObject(Board);
        }

        public CellState[,] DeSerializeBoard(string board)
        {
            return JsonConvert.DeserializeObject<CellState[,]>(board);
        }
        public string GetSingleState(CellState state)
        {
            switch (state)
            {
                case CellState.Empty:
                    return " ";
                case CellState.G:
                    return "G";
                case CellState.R:
                    return "R";
                default:
                    throw new InvalidEnumArgumentException("Unknown Enum option");
            }
        }
    }
}