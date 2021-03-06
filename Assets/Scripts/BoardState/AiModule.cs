﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class AiModule
{
    private static readonly System.Random random = new System.Random();
    private static int moveCheckedCounter = 0;

    public static void AiThreadFunc()
    {
        while(true)
        {

        }
    }

    /// <summary>
    /// Совершить ход.
    /// </summary>
    public static FigureMove AiMove(BoardState boardState)
    {
        BoardState.CheckState checkState = boardState.GetCheckState();
        if(checkState == BoardState.CheckState.mate)
        {
            throw new InvalidOperationException("Невозможно сделать ход: поставлен мат");
        }
        else if(checkState == BoardState.CheckState.stalemate)
        {
            throw new InvalidOperationException("Невозможно сделать ход: пат");
        }
        return Minimax(boardState, 2);
    }

    /// <summary>
    /// Выбрать ход с лучшей оценкой из списка.
    /// </summary>
    /// <param name="boardState">Состояние доски.</param>
    /// <param name="moves">Список ходов.</param>
    private static FigureMove GetBestMove(BoardState boardState, List<FigureMove> moves)
    {
        FigureMove bestMove = null;
        double bestValue = EvaluateBoard(boardState);
        foreach(FigureMove move in moves)
        {
            // Создаем виртуальную доску
            BoardState virtualBoard = new BoardState(boardState);
            // Двигаем фигуру
            virtualBoard.ExecuteMove(move);
            // Оцениваем ход
            double boardValue = EvaluateBoard(virtualBoard);
            if(boardState.turnColor == Figure.FigureColor.white && boardValue > bestValue)
            {
                bestMove = move;
                bestValue = boardValue;
            }
            if(boardState.turnColor == Figure.FigureColor.black && boardValue < bestValue)
            {
                bestMove = move;
                bestValue = boardValue;
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
    /// Возвращает список ходов, доступных после этого хода
    /// </summary>
    public List<FigureMove> GetDerivedMoves(FigureMove move)
    {
        BoardState virtualBoard = move.boardStateAfterMove;
        virtualBoard.UpdateLegalMoves();
        List<FigureMove> derivedMoves = virtualBoard.GetLegalMoves();
        return derivedMoves;
    }

    /// <summary>
    /// Рекурсивная часть функции Minimax.
    /// </summary>
    private static double Alphabeta(FigureMove move, int currentDepth, int maxDepth, double alpha, double beta)
    {
        moveCheckedCounter++;

        // Если достигли конца дерева, получаем оценку доски
        if(currentDepth >= maxDepth)
        {
            return EvaluateBoard(move.boardStateAfterMove);
        }

        // Обновляем список ходов
        move.boardStateAfterMove.UpdateLegalMoves();
        // Получаем оценку каждого хода
        List<FigureMove> moves = move.boardStateAfterMove.GetLegalMoves();
        if(move.color == Figure.FigureColor.white)
        {
            double value = double.NegativeInfinity;
            foreach(FigureMove nextMove in moves)
            {
                value = Math.Max(value, Alphabeta(nextMove, currentDepth + 1, maxDepth, alpha, beta));
                alpha = Math.Max(alpha, value);
                if(alpha >= beta)
                {
                    break;
                }
            }
            return value;
        }
        else
        {
            double value = double.PositiveInfinity;
            foreach(FigureMove nextMove in moves)
            {
                value = Math.Min(value, Alphabeta(nextMove, currentDepth + 1, maxDepth, alpha, beta));
                beta = Math.Min(beta, value);
                if(beta <= alpha)
                {
                    break;
                }
            }
            return value;
        }
    }

    /// <summary>
    /// Возвращает ход с лучшей оценкой с помощью алгоритма минимакс.
    /// </summary>
    private static FigureMove Minimax(BoardState boardState, int maxDepth)
    {
        // Список разрешенных ходов
        List<FigureMove> availableMoves = boardState.GetLegalMoves();
        // Оценки ходов
        moveCheckedCounter = 0;
        List<double> moveValues = availableMoves.Select(move => Alphabeta(move, 0, maxDepth, double.PositiveInfinity, double.NegativeInfinity)).ToList();
        UnityEngine.Debug.Log($"{moveCheckedCounter} moves checked");
        // Значение лучшей оценки
        double bestMoveValue = boardState.turnColor == Figure.FigureColor.white ? moveValues.Max() : moveValues.Min();
        // Выбираем ходы с лучшей оценкой
        List<FigureMove> movesWithBestvalue = new List<FigureMove>();
        for(int i = 0; i < moveValues.Count; i++)
        {
            if(moveValues[i] == bestMoveValue)
            {
                movesWithBestvalue.Add(availableMoves[i]);
            }
        }
        // Выбираем случайный ход из ходов с лучшей оценкой
        FigureMove bestMove = movesWithBestvalue[random.Next(movesWithBestvalue.Count)];
        return bestMove;
    }

    /// <summary>
    /// Возвращает ход с лучшей оценкой.
    /// </summary>
    private static FigureMove BestEvaluationMove(BoardState boardState)
    {
        // Ищем ход с лучшей оценкой
        List<FigureMove> availableMoves = boardState.GetLegalMoves();
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
    private static FigureMove RandomMove(BoardState boardState)
    {
        List<FigureMove> availableMoves = boardState.moveList.FindAll(move => move.attackingFigures.Count == 0);
        FigureMove randomMove = availableMoves[random.Next(availableMoves.Count)];
        return randomMove;
    }

    /// <summary>
    /// Оценить состояние доски.
    /// </summary>
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
