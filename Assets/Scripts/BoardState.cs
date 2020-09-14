using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class Figure
{
    protected int x;
    protected int y;
    public GameObject gameObject;
    public string color;
    public BoardState boardState;
    public int moveCount = 0;

    protected List<(int, int)> tempLegalMoveCells = new List<(int, int)>();

    public abstract List<(int, int)> GetMoveCells();

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
        gameObject.transform.position = new Vector3(x, y);
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

    public void Delete()
    {
        UnityEngine.Object.Destroy(gameObject);
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
            tempLegalMoveCells.Add((x, y));
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

    public override List<(int, int)> GetMoveCells()
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

    public override List<(int, int)> GetMoveCells()
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

    public override List<(int, int)> GetMoveCells()
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

    public override List<(int, int)> GetMoveCells()
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

    public override List<(int, int)> GetMoveCells()
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

    public override List<(int, int)> GetMoveCells()
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

    public static bool CoordinatesInBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    public Figure GetFigureAtCell(int x, int y)
    {
        return CoordinatesInBounds(x, y) ? board[x, y] : null;
    }

    public void SetFigureAtCell(int x, int y, Figure figure)
    {
        if(!CoordinatesInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException("Координаты за пределами доски");
        }
        board[x, y] = figure;
    }

}
