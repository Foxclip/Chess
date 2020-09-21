using System;
using System.Collections;
using System.Collections.Generic;

public class AiModule
{
    private static Random random = new Random();

    /// <summary>
    /// Совершить ход.
    /// </summary>
    public static void AiMove(BoardState boardState)
    {
        // Если мат то ходить некуда
        if(boardState.DetectMate())
        {
            return;
        }
        // Случайный ход
        List<FigureMove> availableMoves = boardState.GetMovesByColor(boardState.turnColor, true);
        FigureMove randomMove = availableMoves[random.Next(availableMoves.Count)];
        boardState.ExecuteMove(randomMove);
    }

}
