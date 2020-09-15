﻿using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;

public struct FigureMove
{
    public Vector2Int from;
    public Vector2Int to;

    public FigureMove(Vector2Int from, Vector2Int to)
    {
        this.from = from;
        this.to = to;
    }
}

public abstract class Figure
{
    public int x;
    public int y;
    public Vector2Int Pos { get => new Vector2Int(x, y); }
    public GameObject gameObject;
    public string color;
    public BoardState boardState;
    public int moveCount = 0;

    protected List<Vector2Int> tempLegalMoveCells = new List<Vector2Int>();


    public abstract List<Vector2Int> GetMoveCells();

    public Figure(int x, int y, string color, BoardState boardState)
    {
        if(!BoardState.CoordinatesInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException("Нельзя создать фигуру за пределами доски");
        }
        if(!color.Equals("white") && !color.Equals("black"))
        {
            throw new ArgumentOutOfRangeException("Цвет фигуры должен быть white или black");
        }
        this.x = x;
        this.y = y;
        this.color = color;
        this.boardState = boardState ?? throw new ArgumentNullException("boardState");
        boardState.SetFigureAtCell(x, y, this);
    }

    public Figure Copy(BoardState boardState)
    {
        Figure copy = (Figure)GetType().GetConstructors()[0].Invoke(new object[] { x, y, color, boardState, false });
        copy.moveCount = moveCount;
        return copy;
    }

    public void Move(int newX, int newY)
    {
        if(!BoardState.CoordinatesInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException("Нельзя передвинуть фигуру за пределы доски");
        }
        // Изменяем параметры фигуры
        int oldX = x;
        int oldY = y;
        x = newX;
        y = newY;
        if(gameObject != null)
        {
            gameObject.transform.position = new Vector3(x, y);
        }
        moveCount++;
        // Если в клетке уже есть фигура
        Figure figureAtCell = boardState.GetFigureAtCell(x, y);
        if(figureAtCell != null)
        {
            figureAtCell.Delete();
        }
        // Изменяем состояние доски
        boardState.SetFigureAtCell(oldX, oldY, null);
        boardState.SetFigureAtCell(newX, newY, this);
    }

    public void Move(Vector2Int cell)
    {
        Move(cell.x, cell.y);
    }

    public void Delete()
    {
        if(gameObject != null)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
        boardState.SetFigureAtCell(x, y, null);
    }

    public string GetEnemyColor()
    {
        return color.Equals("white") ? "black" : "white";
    }

    public void TestCell(int x, int y, bool freeMove = true, bool takePieces = true)
    {
        Figure figureAtCell = boardState.GetFigureAtCell(x, y);
        bool inBounds = BoardState.CoordinatesInBounds(x, y);
        bool canFreeMove = freeMove && figureAtCell == null;
        bool canTakePiece = takePieces && figureAtCell != null && figureAtCell.color.Equals(GetEnemyColor());
        if((canFreeMove || canTakePiece) && inBounds)
        {
            tempLegalMoveCells.Add(new Vector2Int(x, y));
        }
    }

    public void TestDirection(int directionX, int directionY)
    {
        if(directionX > 1 || directionX < -1 || directionY > 1 || directionX < -1)
        {
            throw new ArgumentOutOfRangeException("Направление может принимать значения -1, 0 или 1");
        }
        for(int offset = 1; ; offset++)
        {
            int xOffset = offset * directionX;
            int yOffset = offset * directionY;
            int cellX = x + xOffset;
            int cellY = y + yOffset;
            TestCell(cellX, cellY, takePieces: true);
            bool ranIntoAFigure = boardState.GetFigureAtCell(cellX, cellY) != null;
            bool ranOutOfBounds = !BoardState.CoordinatesInBounds(cellX, cellY);
            if(ranIntoAFigure || ranOutOfBounds)
            {
                break;
            }
        }
    }

    public void LoadGameObject(string name)
    {
        if(gameObject != null)
        {
            throw new InvalidOperationException("GameObject уже загружен");
        }
        gameObject = UnityEngine.Object.Instantiate(Resources.Load(name)) as GameObject;
        gameObject.transform.position = new Vector3(x, y);
        gameObject.transform.parent = GameObject.Find("pieces").transform;
        PieceController pieceController = gameObject.GetComponent<PieceController>();
        pieceController.boardStateFigure = this;
    }
}

public class Pawn: Figure
{
    public Pawn(int x, int y, string color, BoardState boardState, bool createGameObject = false): base(x, y, color, boardState)
    {
        if(createGameObject)
        {
            LoadGameObject($"{color}_pawn");
        }
    }

    public override List<Vector2Int> GetMoveCells()
    {
        tempLegalMoveCells.Clear();
        int direction = color.Equals("white") ? 1 : -1;

        TestCell(x, y + direction, takePieces: false);
        if(moveCount == 0 && boardState.GetFigureAtCell(x, y + direction) == null)
        {
            TestCell(x, y + direction * 2, takePieces: false);
        }
        TestCell(x - 1, y + direction, freeMove: false);
        TestCell(x + 1, y + direction, freeMove: false);

        return tempLegalMoveCells;
    }

}

public class Rook : Figure
{
    public Rook(int x, int y, string color, BoardState boardState, bool createGameObject = false) : base(x, y, color, boardState)
    {
        if(createGameObject)
        {
            LoadGameObject($"{color}_rook");
        }
    }

    public override List<Vector2Int> GetMoveCells()
    {
        tempLegalMoveCells.Clear();
        TestDirection(0, 1);
        TestDirection(1, 0);
        TestDirection(0, -1);
        TestDirection(-1, 0);
        return tempLegalMoveCells;
    }

}

public class Knight : Figure
{
    public Knight(int x, int y, string color, BoardState boardState, bool createGameObject = false) : base(x, y, color, boardState)
    {
        if(createGameObject)
        {
            LoadGameObject($"{color}_knight");
        }
    }

    public override List<Vector2Int> GetMoveCells()
    {
        tempLegalMoveCells.Clear();
        TestCell(x - 1, y + 2);
        TestCell(x + 1, y + 2);
        TestCell(x + 2, y - 1);
        TestCell(x + 2, y + 1);
        TestCell(x - 1, y - 2);
        TestCell(x + 1, y - 2);
        TestCell(x - 2, y + 1);
        TestCell(x - 2, y - 1);
        return tempLegalMoveCells;
    }

}

public class Bishop : Figure
{
    public Bishop(int x, int y, string color, BoardState boardState, bool createGameObject = false) : base(x, y, color, boardState)
    {
        if(createGameObject)
        {
            LoadGameObject($"{color}_bishop");
        }
    }

    public override List<Vector2Int> GetMoveCells()
    {
        tempLegalMoveCells.Clear();
        TestDirection(1, 1);
        TestDirection(1, -1);
        TestDirection(-1, -1);
        TestDirection(-1, 1);
        return tempLegalMoveCells;
    }

}

public class King : Figure
{
    public King(int x, int y, string color, BoardState boardState, bool createGameObject = false) : base(x, y, color, boardState)
    {
        if(createGameObject)
        {
            LoadGameObject($"{color}_king");
        }
    }

    public override List<Vector2Int> GetMoveCells()
    {
        tempLegalMoveCells.Clear();
        TestCell(x - 1, y - 1);
        TestCell(x - 1, y + 0);
        TestCell(x - 1, y + 1);
        TestCell(x + 0, y + 1);
        TestCell(x + 1, y + 1);
        TestCell(x + 1, y + 0);
        TestCell(x + 1, y - 1);
        TestCell(x + 0, y - 1);
        return tempLegalMoveCells;
    }

}

public class Queen : Figure
{
    public Queen(int x, int y, string color, BoardState boardState, bool createGameObject = false) : base(x, y, color, boardState)
    {
        if(createGameObject)
        {
            LoadGameObject($"{color}_queen");
        }
    }

    public override List<Vector2Int> GetMoveCells()
    {
        tempLegalMoveCells.Clear();
        TestDirection(0, 1);
        TestDirection(1, 0);
        TestDirection(0, -1);
        TestDirection(-1, 0);
        TestDirection(1, 1);
        TestDirection(1, -1);
        TestDirection(-1, -1);
        TestDirection(-1, 1);
        return tempLegalMoveCells;
    }

}

public class BoardState
{

    private readonly Figure[,] board = new Figure[8, 8];
    public readonly List<FigureMove> legalMoves = new List<FigureMove>();

    public BoardState()
    {
        // Белые пешки
        for(int x = 0; x < 8; x++)
        {
            new Pawn(x, 1, "white", this, true);
        }
        // Черные пешки
        for(int x = 0; x < 8; x++)
        {
            new Pawn(x, 6, "black", this, true);
        }
        // Ладьи
        new Rook(0, 0, "white", this, true);
        new Rook(7, 0, "white", this, true);
        new Rook(0, 7, "black", this, true);
        new Rook(7, 7, "black", this, true);
        // Кони
        new Knight(1, 0, "white", this, true);
        new Knight(6, 0, "white", this, true);
        new Knight(1, 7, "black", this, true);
        new Knight(6, 7, "black", this, true);
        // Слоны
        new Bishop(2, 0, "white", this, true);
        new Bishop(5, 0, "white", this, true);
        new Bishop(2, 7, "black", this, true);
        new Bishop(5, 7, "black", this, true);
        // Короли
        new King(3, 0, "white", this, true);
        new King(3, 7, "black", this, true);
        // Ферзи
        new Queen(4, 0, "white", this, true);
        new Queen(4, 7, "black", this, true);
    }

    public BoardState(BoardState original)
    {
        board = new Figure[8, 8];
        for(int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 8; y++)
            {
                if(original.board[x, y] != null)
                {
                    board[x, y] = original.board[x, y].Copy(this);
                }
            }
        }
    }

    public static bool CoordinatesInBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    public Figure GetFigureAtCell(int x, int y)
    {
        return CoordinatesInBounds(x, y) ? board[x, y] : null;
    }

    public Figure GetFigureAtCell(Vector2Int cell)
    {
        return GetFigureAtCell(cell.x, cell.y);
    }

    public void SetFigureAtCell(int x, int y, Figure figure)
    {
        if(!CoordinatesInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException("Координаты за пределами доски");
        }
        board[x, y] = figure;
    }

    public bool DetectCheck(string color)
    {
        string enemyColor = color.Equals("black") ? "white" : "black";
        // Получаем фигуры
        List<Figure> figures = (from Figure figure in board where figure != null select figure).ToList();
        List<Figure> ownFigures = (from Figure figure in figures where figure.color == color select figure).ToList();
        List<Figure> enemyFigures = (from Figure figure in figures where figure.color == enemyColor select figure).ToList();
        King king = (King)(from Figure figure in ownFigures where figure.GetType() == typeof(King) select figure).First();
        // Ходы короля
        List<Vector2Int> kingMoves = king.GetMoveCells();
        // Ходы вражеских фигур
        List<Vector2Int> enemyMoves = new List<Vector2Int>();
        foreach(Figure figure in enemyFigures)
        {
            List<Vector2Int> moves = figure.GetMoveCells();
            enemyMoves = enemyMoves.Concat(moves).ToList();
        }
        enemyMoves = enemyMoves.Distinct().ToList();
        // Определение шаха
        return enemyMoves.Contains(new Vector2Int(king.x, king.y));
    }

    public bool DetectMate()
    {
        // Определение мата
        return legalMoves.Count == 0;
    }

    public void UpdateLegalMoves(string color)
    {
        legalMoves.Clear();
        // Получаем фигуры
        List<Figure> figures = (from Figure figure in board where figure != null select figure).ToList();
        List<Figure> ownFigures = (from Figure figure in figures where figure.color == color select figure).ToList();
        // Получаем свои ходы
        List<FigureMove> ownMoves = new List<FigureMove>();
        foreach(Figure figure in ownFigures)
        {
            List<Vector2Int> moves = figure.GetMoveCells();
            foreach(Vector2Int move in moves)
            {
                ownMoves.Add(new FigureMove(figure.Pos, move));
            }
        }
        ownMoves = ownMoves.Distinct().ToList();
        // Пытаемся сделать ход
        foreach(FigureMove ownMove in ownMoves)
        {
            BoardState virtualBoard = new BoardState(this);
            Figure figure = virtualBoard.GetFigureAtCell(ownMove.from);
            figure.Move(ownMove.to);
            if(!virtualBoard.DetectCheck(color))
            {
                legalMoves.Add(ownMove);
            }
        }
    }

}
