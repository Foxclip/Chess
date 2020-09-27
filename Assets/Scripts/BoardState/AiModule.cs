using System;
using System.Collections;
using System.Collections.Generic;

public class AiModule
{
    private static readonly Random random = new Random();

    /// <summary>
    /// Совершить ход.
    /// </summary>
    public static FigureMove AiMove(BoardState boardState)
    {
        if(boardState.DetectMate())
        {
            throw new InvalidOperationException("Невозможно сделать ход: поставлен мат");
        }
        // Случайный ход
        List<FigureMove> availableMoves = boardState.moveList.FindAll(move => move.attackingFigures.Count == 0);
        FigureMove randomMove = availableMoves[random.Next(availableMoves.Count)];
        return randomMove;
    }

}
