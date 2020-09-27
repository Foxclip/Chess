using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AiModule
{
    private static readonly System.Random random = new System.Random();

    /// <summary>
    /// Совершить ход.
    /// </summary>
    public static FigureMove AiMove(BoardState boardState)
    {
        if(boardState.DetectMate())
        {
            throw new InvalidOperationException("Невозможно сделать ход: поставлен мат");
        }
        // Ищем ход с лучшей оценкой
        List<FigureMove> availableMoves = boardState.moveList.FindAll(move => move.attackingFigures.Count == 0);
        FigureMove bestMove = null;
        double bestValue = EvaluateBoard(boardState);
        foreach(FigureMove move in availableMoves)
        {
            double boardValueAfterMove = EvaluateBoard(move.boardStateAfterMove);
            if(boardState.turnColor == Figure.FigureColor.white && boardValueAfterMove > bestValue)
            {
                bestMove = move;
                bestValue = boardValueAfterMove;
            }
            if(boardState.turnColor == Figure.FigureColor.black && boardValueAfterMove < bestValue)
            {
                bestMove = move;
                bestValue = boardValueAfterMove;
            }
        }
        // Если нет хода улучшающего оценку доски, берем случайный ход
        if(bestMove is null)
        {
            return RandomMove(boardState);
        }
        return bestMove;
    }

    /// <summary>
    /// Делает случайный ход.
    /// </summary>
    /// <param name="boardState"></param>
    private static FigureMove RandomMove(BoardState boardState)
    {
        List<FigureMove> availableMoves = boardState.moveList.FindAll(move => move.attackingFigures.Count == 0);
        FigureMove randomMove = availableMoves[random.Next(availableMoves.Count)];
        return randomMove;
    }

    /// <summary>
    /// Оценить состояние доски.
    /// </summary>
    /// <returns></returns>
    public static double EvaluateBoard(BoardState boardState)
    {
        double sum = 0.0;
        foreach(Figure figure in boardState.figures)
        {
            double pieceValue = 0.0;
            switch(figure.GetType().Name)
            {
                case nameof(Pawn):   pieceValue =  10; break;
                case nameof(Knight): pieceValue =  30; break;
                case nameof(Bishop): pieceValue =  30; break;
                case nameof(Rook):   pieceValue =  50; break;
                case nameof(Queen):  pieceValue =  90; break;
                case nameof(King):   pieceValue = 900; break;
            }
            if(figure.color == Figure.FigureColor.black)
            {
                pieceValue = -pieceValue;
            }
            sum += pieceValue;
        }
        return sum;
    }

}
