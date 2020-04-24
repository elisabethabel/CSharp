﻿using System;
using System.Collections.Generic;
 using System.Threading;
 using ConsoleUI;
 using DAL;
 using Domain;
 using GameEngine;
 using MenuSystem;
 using Microsoft.EntityFrameworkCore;
 namespace ConsoleApp
{
    class Program
    {
        private static GameSettings _settings = default!;
        private static DbContextOptions _options = default!;
        
        static void Main()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(@"Data source=C:\Users\admin\OneDrive\Dokumendid\icd0008\icd0008-2019f\Connect4\WebApp\connect4Db.db")
                .Options;

            Console.Clear();
            
            var humanVsHumanMenu = new Menu(2)
            {
                Title = "Human vs Human",
                MenuItemsDictionary = new Dictionary<string, MenuItem>()
                {
                    {"1", new MenuItem()
                    {
                        Title = "Player 1 starts",
                        CommandToExecute = (() => NewGame(true, false))
                    }},
                    {"2", new MenuItem()
                    {
                        Title = "Player 2 starts",
                        CommandToExecute = (() => NewGame(false, false))
                    }}
                }
            };
            
            var humanVsComputerMenu = new Menu(2)
            {
                Title = "Human vs Computer",
                MenuItemsDictionary = new Dictionary<string, MenuItem>()
                {
                    {"1", new MenuItem()
                    {
                        Title = "Human Starts",
                        CommandToExecute = (() => NewGame(true, true))
                    }},
                    {"2", new MenuItem()
                    {
                        Title = "Computer starts",
                        CommandToExecute = (() => NewGame(false, true))
                    }}
                }
            };
            
            var startGameMenu = new Menu(1)
            {
                Title = "Start a new game of Connect 4",
                MenuItemsDictionary = new Dictionary<string, MenuItem>()
                {
                    {
                        "1", new MenuItem()
                        {
                            Title = "Human vs Computer",
                            CommandToExecute = humanVsComputerMenu.Run

                        }
                    },
                    {
                        "2", new MenuItem()
                        {
                            Title = "Human vs Human",
                            CommandToExecute = humanVsHumanMenu.Run
                        }
                    }
                }
            };

            var mainMenu = new Menu(0)
            {
                Title = "Connect 4 Main Menu",
                MenuItemsDictionary = new Dictionary<string, MenuItem>()
                {
                    {
                        "N", new MenuItem()
                        {
                            Title = "Start a new game",
                            CommandToExecute = startGameMenu.Run

                        }
                    },
                    {
                        "L", new MenuItem()
                        {
                            Title = "Load a previous Game",
                            CommandToExecute = (() => LoadPreviousGamesMenu().Run())

                        }
                    },
                    {
                        "S", new MenuItem()
                        {
                            Title = "Game Settings",
                            CommandToExecute = SaveSettings

                        }
                    }
                }
            };

            mainMenu.Run();
        }

        private static Menu LoadPreviousGamesMenu()
        {
            Dictionary<string, MenuItem> menuItems = new Dictionary<string, MenuItem>();
            var menuItemCommand = 1;
            
            using (var ctx = new AppDbContext(_options))
            {
                foreach (var game in ctx.SavedGames)
                {
                    menuItems.Add(menuItemCommand.ToString(),
                        new MenuItem()
                        {
                            Title = game.GameName + " | " + game.PlayerOneName + " vs " + game.PlayerTwoName,
                            CommandToExecute = (() => LoadGame(game.SavedGameId))
                        });
                    menuItemCommand += 1;
                }
            }

            var savedGamesMenu = new Menu(1)
            {
                Title = "Load a previous game",
                MenuItemsDictionary = menuItems
            };
            return savedGamesMenu;


        }

        static string SaveSettings()
        {
            Console.Clear();

            var boardWidth = 0;
            var boardHeight = 0;
            var winningCondition = 0;
            var userCanceled = false;

            (boardHeight, userCanceled) = GetUserIntInput("Enter number of rows (minimum is 4 and maximum is 30) :", 4, 30);
            if (userCanceled) return "";

            (boardWidth, userCanceled) = GetUserIntInput("Enter number of columns (minimum is 5 and maximum is 30) :", 5, 30);
            if (userCanceled) return "";

            var maxWinningCondition = 0;
            if (boardHeight > boardWidth || boardHeight == boardWidth)
            {
                maxWinningCondition = boardHeight;
            }
            else
            {
                maxWinningCondition = boardWidth;
            }
            (winningCondition, userCanceled) = GetUserIntInput("Enter how many chips in a row is needed to win :", 2, maxWinningCondition);
            if (userCanceled) return "";

            _settings = new GameSettings()
            {
                BoardHeight = boardHeight,
                BoardWidth = boardWidth,
                WinningCondition = winningCondition
            };
            GameConfigHandler.SaveConfig(_settings);
            return "";
        }

        static string NewGame(bool playerOneMoves, bool isHumanVsComputer)
        {
            _settings = GameConfigHandler.LoadConfig();
            var game = new Game(new AppDbContext(_options));
            string playerOneName;
            string playerTwoName;
            if (!isHumanVsComputer)
            {
                playerOneName = GetUserStrInput("Enter the name of player 1:");
                playerTwoName = GetUserStrInput("Enter the name of player 2:");
            }
            else
            {
                playerOneName = GetUserStrInput("Enter your player name:");
                playerTwoName = "Computer";
            }

            var gameName = GetUserStrInput("Enter the name of your game!");
            _settings.IsHumanVsComputer = isHumanVsComputer;
            game.InitializeNewGame(_settings.BoardHeight, _settings.BoardWidth);
            game.GameName = gameName;
            game.PlayerOneMoves = playerOneMoves;
            game.PlayerOneName = playerOneName;
            game.PlayerTwoName = playerTwoName;
            return Connect4Game(game);
        }
        static string LoadGame(int gameId)
        {
            var game = new Game(new AppDbContext(_options));
            SavedGame savedGame = game.RestoreSavedGameFromDb(gameId);
            _settings = new GameSettings()
            {
                BoardHeight = game.BoardHeight,
                BoardWidth = game.BoardWidth,
                IsHumanVsComputer = savedGame.IsHumanVsComputer,
                WinningCondition = savedGame.WinningCondition
            };
            return Connect4Game(game);
        }

        static string Connect4Game(Game game)
        {
            if (game.GameIsOver)
            {
                Console.Clear();
                GameUi.PrintBoard(game);
                Console.WriteLine(game.Message);
            }
            else
            { 
                game.SaveGameToDb(_settings.IsHumanVsComputer, _settings.WinningCondition);
                do 
                { 
                    Console.Clear(); 
                    GameUi.PrintBoard(game); 
                    int posX; 
                    int posY; 
                    var player = game.PlayerOneMoves ? game.PlayerOneName : game.PlayerTwoName;
                    
                    if (_settings.IsHumanVsComputer && !game.PlayerOneMoves) 
                    { 
                        Console.WriteLine(player + "'s turn"); 
                        for(int i = 0;i < 3;i++) 
                        { 
                            Console.Write("."); 
                            Thread.Sleep(600); 
                        } 
                        posX = game.ComputerMove(_settings.WinningCondition); 
                        posY = game.FindYIndex(posX); 
                    }
                    else 
                    { 
                        var correctXInt = false; 
                        do 
                        { 
                            bool userCanceled; 
                            (posX, userCanceled) = GetUserIntInput( $"{player} enter X coordinate (1 - {_settings.BoardWidth})",
                            1, _settings.BoardWidth); 
                            posX -= 1; 
                            posY = game.FindYIndex(posX); 
                            if (userCanceled) 
                            { 
                                correctXInt = true; 
                                game.GameIsOver = true; 
                            }
                            else 
                            { 
                                if (posY != -1) 
                                { 
                                    correctXInt = true; 
                                }
                                else 
                                { 
                                    Console.WriteLine(posX + 1 + " column is already full!"); 
                                } 
                            } 
                        } while (!correctXInt); 
                    } 
                    if (!game.GameIsOver) 
                    { 
                        game.Move(posY, posX);
                        CellState playerState = game.PlayerOneMoves ? CellState.G : CellState.R; 
                        if (game.IsWinning(posY, posX, _settings.WinningCondition, playerState)) 
                        { 
                            game.GameIsOver = true; 
                            game.Message = player + " has won the game!";
                        }
                        else if (game.TableIsFull()) 
                        { 
                            game.GameIsOver = true; 
                            game.Message = "IT'S A TIE'!"; 
                        } 
                    }
                    game.SaveGameToDb(_settings.IsHumanVsComputer, _settings.WinningCondition); 
                } while (!game.GameIsOver);
                Console.Clear();
                GameUi.PrintBoard(game);
                Console.WriteLine(game.Message);
            }
            return game.Message;
        }

        static string GetUserStrInput(string prompt)
        {
            do
            {
                Console.WriteLine(prompt);
                Console.Write(">");
                var userStr = Console.ReadLine();
                if (string.IsNullOrEmpty(userStr))
                {
                    Console.WriteLine("This is not a correct name!");
                }
                else
                {
                    return userStr;
                }
            } while (true);
        }

        static (int result, bool wasCanceled) GetUserIntInput(string prompt, int min, int max, int? cancelIntValue =
            null, string cancelStrValue = "Q")
        {
            do
            {
                Console.WriteLine(prompt);
                if (cancelIntValue.HasValue || !string.IsNullOrWhiteSpace(cancelStrValue))
                {
                    Console.WriteLine($"To quit enter: {cancelIntValue}" +
                                      $"{(cancelIntValue.HasValue && !string.IsNullOrWhiteSpace(cancelStrValue) ? " or " : "")}" +
                                      $"{cancelStrValue}");
                }

                Console.Write(">");
                var consoleLine = Console.ReadLine();

                if (consoleLine == cancelStrValue) return (0, true);

                if (int.TryParse(consoleLine, out var userInt))
                {
                    if (userInt < min || userInt > max)
                    {
                        Console.WriteLine($"'{userInt}' is not a correct index!");
                    }
                    else
                    {
                        return userInt == cancelIntValue ? (userInt, true) : (userInt, false);
                    }
                }
                else
                {
                    Console.WriteLine($"'{consoleLine}' cant be converted to int value!");
                }
            } while (true);
        }
    }
}
