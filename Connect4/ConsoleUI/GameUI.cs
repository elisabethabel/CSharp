using System;
using System.ComponentModel;
using Domain;
using GameEngine;

namespace ConsoleUI
{
    public static class GameUi
    {
        private static readonly string _verticalSeparator = "|";
        private static readonly string _horizontalSeparator = "-";
        private static readonly string _centerSeparator = "+";

        public static void PrintBoard(Game game)
        {
            var board = game.GetBoard();
            for (int yIndex = 0; yIndex < game.BoardHeight; yIndex++)
            {
                var line = "";
                var topAndBottomLine = "";
                var xIndexes = "";
                for (int xIndex = 0; xIndex < game.BoardWidth; xIndex++)
                {
                    topAndBottomLine = topAndBottomLine
                                       + _horizontalSeparator + _horizontalSeparator
                                       + _horizontalSeparator + _horizontalSeparator;
                    line = line + _verticalSeparator + " " + game.GetSingleState(board[yIndex, xIndex]) + " ";
                    if (xIndex == game.BoardWidth - 1)
                    {
                        line = line + _verticalSeparator;
                        topAndBottomLine += _horizontalSeparator;
                    }

                    if (yIndex == 0)
                    {
                        xIndexes = xIndexes +  "  " + (xIndex + 1) + " ";
                    }
                }

                if (yIndex == 0)
                {
                    Console.WriteLine(xIndexes);
                    Console.WriteLine(topAndBottomLine);
                }
                Console.WriteLine(line);
                
                line = "";
                if (yIndex < game.BoardHeight - 1)
                {
                    for (int xIndex = 0; xIndex < game.BoardWidth; xIndex++)
                    {
                        {
                            if (xIndex == 0 || xIndex == game.BoardWidth - 1)
                            {
                                line += _horizontalSeparator;
                            }
                            line = line + _horizontalSeparator + _horizontalSeparator + _horizontalSeparator;
                            if (xIndex < game.BoardWidth - 1)
                            {
                                line += _centerSeparator;   
                            }
                        }
                    }
                    Console.WriteLine(line);
                }

                if (yIndex == game.BoardHeight - 1)
                {
                    Console.WriteLine(topAndBottomLine);
                }
            }
        }
    }
}