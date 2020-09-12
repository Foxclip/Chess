using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Абстрактный класс фигуры.
/// </summary>
public abstract class Figure
{
    /// <summary>
    /// Конструктор класса Figure.
    /// </summary>
    /// <param name="x">Координата x фигуры.</param>
    /// <param name="y">Координата y фигуры.</param>
    /// <param name="color">Цвет фигуры. Может быть white или black.</param>
    /// <param name="boardState">Доска, на которой будет находится фигура.</param>
    public Figure(int x, int y, string color, BoardState boardState)
    {
        if (!BoardState.CoordinatesInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException("Нельзя создать фигуру за пределами доски");
        }
        if (!color.Equals("white") && !color.Equals("black"))
        {
            throw new ArgumentOutOfRangeException("Цвет фигуры должен быть white или black");
        }
        this.X = x;
        this.Y = y;
        this.Color = color;
        this.BoardState = boardState ?? throw new ArgumentNullException("boardState");
        boardState.SetFigureAtCell(x, y, this);
    }

    /// <summary>
    /// Привязанный GameObject. Может быть null.
    /// </summary>
    public GameObject GameObject { get; set; }

    /// <summary>
    /// Цвет фигуры. Может быть white или black.
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Доска, на которой находится фигура.
    /// </summary>
    public BoardState BoardState { get; set; }

    /// <summary>
    /// Количество ходов, совершенное фигурой с начала партии.
    /// </summary>
    public int MoveCount { get; set; } = 0;

    /// <summary>
    /// Временный список, используется функцией GetMoveCells.
    /// </summary>
    protected List<(int, int)> TempLegalMoveCells { get; set; } = new List<(int, int)>();

    /// <summary>
    /// Координата x фигуры.
    /// </summary>
    protected int X { get; set; }

    /// <summary>
    /// Координата y фигуры.
    /// </summary>
    protected int Y { get; set; }

    /// <summary>
    /// Получает список возможных ходов фигуры.
    /// </summary>
    /// <returns>Список возможных ходов фигуры.</returns>
    public abstract List<(int, int)> GetMoveCells();

    /// <summary>
    /// Двигает фигуру на другую клетку.
    /// </summary>
    /// <param name="newX">Координата x клетки.</param>
    /// <param name="newY">Координата y клетки.</param>
    public void Move(int newX, int newY)
    {
        if (!BoardState.CoordinatesInBounds(this.X, this.Y))
        {
            throw new ArgumentOutOfRangeException("Нельзя передвинуть фигуру за пределы доски");
        }

        // Изменяем параметры фигуры
        int oldX = this.X;
        int oldY = this.Y;
        this.X = newX;
        this.Y = newY;
        this.GameObject.transform.position = new Vector3(this.X, this.Y);
        this.MoveCount++;

        // Если в клетке уже есть фигура
        Figure figureAtCell = this.BoardState.GetFigureAtCell(this.X, this.Y);
        if (figureAtCell != null)
        {
            figureAtCell.Delete();
        }

        // Изменяем состояние доски
        this.BoardState.SetFigureAtCell(oldX, oldY, null);
        this.BoardState.SetFigureAtCell(newX, newY, this);
    }

    /// <summary>
    /// Удаляет фигуу.
    /// </summary>
    public void Delete()
    {
        UnityEngine.Object.Destroy(this.GameObject);
        this.BoardState.SetFigureAtCell(this.X, this.Y, null);
    }

    /// <summary>
    /// Возвращает вражеский цвет.
    /// </summary>
    /// <returns>Цвет вражеских фигур.</returns>
    public string GetEnemyColor()
    {
        return this.Color.Equals("white") ? "black" : "white";
    }

    /// <summary>
    /// Прроверяет, может ли фигура совершить ход в эту клетку.
    /// </summary>
    /// <param name="x">Координата x клетки.</param>
    /// <param name="y">Координата y клетки.</param>
    /// <param name="freeMove">Ход в свободную клетку.</param>
    /// <param name="takePieces">Взятие фигуры.</param>
    public void TestCell(int x, int y, bool freeMove = true, bool takePieces = true)
    {
        Figure figureAtCell = this.BoardState.GetFigureAtCell(x, y);
        bool inBounds = BoardState.CoordinatesInBounds(x, y);
        bool canFreeMove = freeMove && figureAtCell == null;
        bool canTakePiece = takePieces && figureAtCell != null && figureAtCell.Color.Equals(this.GetEnemyColor());
        if ((canFreeMove || canTakePiece) && inBounds)
        {
            this.TempLegalMoveCells.Add((x, y));
        }
    }

    /// <summary>
    /// Возвращает список клеток в которые фигура может пойти в определенном направлении.
    /// </summary>
    /// <param name="directionX">Сдвиг по x.</param>
    /// <param name="directionY">Сдвиг по y.</param>
    public void TestDirection(int directionX, int directionY)
    {
        if (directionX > 1 || directionX < -1 || directionY > 1 || directionX < -1)
        {
            throw new ArgumentOutOfRangeException("Направление может принимать значения -1, 0 или 1");
        }
        for (int offset = 1; ; offset++)
        {
            int xOffset = offset * directionX;
            int yOffset = offset * directionY;
            int cellX = this.X + xOffset;
            int cellY = this.Y + yOffset;
            this.TestCell(cellX, cellY, takePieces: true);
            bool ranIntoAFigure = this.BoardState.GetFigureAtCell(cellX, cellY) != null;
            bool ranOutOfBounds = !BoardState.CoordinatesInBounds(cellX, cellY);
            if (ranIntoAFigure || ranOutOfBounds)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Загружает GameObject и добаляет его в сцену.
    /// </summary>
    /// <param name="name">Имя префаба.</param>
    public void LoadGameObject(string name)
    {
        if (this.GameObject != null)
        {
            throw new InvalidOperationException("GameObject уже загружен");
        }
        this.GameObject = UnityEngine.Object.Instantiate(Resources.Load(name)) as GameObject;
        this.GameObject.transform.position = new Vector3(this.X, this.Y);
        this.GameObject.transform.parent = GameObject.Find("pieces").transform;
        PieceController pieceController = this.GameObject.GetComponent<PieceController>();
        pieceController.boardStateFigure = this;
    }
}

/// <summary>
/// Класс пешки.
/// </summary>
public class Pawn : Figure
{
    /// <summary>
    /// Констуктор класса Pawn.
    /// </summary>
    /// <param name="x">Координата x.</param>
    /// <param name="y">Координата y.</param>
    /// <param name="color">Цвет.</param>
    /// <param name="boardState">Доска.</param>
    /// <param name="createGameObject">Создать ли GameObject.</param>
    public Pawn(int x, int y, string color, BoardState boardState, bool createGameObject = false)
        : base(x, y, color, boardState)
    {
        if (createGameObject)
        {
            this.LoadGameObject($"{color}_pawn");
        }
    }

    /// <inheritdoc/>
    public override List<(int, int)> GetMoveCells()
    {
        this.TempLegalMoveCells.Clear();
        int direction = this.Color.Equals("white") ? 1 : -1;

        this.TestCell(this.X, this.Y + direction, takePieces: false);
        if (this.MoveCount == 0 && this.BoardState.GetFigureAtCell(this.X, this.Y + direction) == null)
        {
            this.TestCell(this.X, this.Y + (direction * 2), takePieces: false);
        }
        this.TestCell(this.X - 1, this.Y + direction, freeMove: false);
        this.TestCell(this.X + 1, this.Y + direction, freeMove: false);

        return this.TempLegalMoveCells;
    }
}

/// <summary>
/// Класс ладьи.
/// </summary>
public class Rook : Figure
{
    /// <summary>
    /// Констуктор класса Rook.
    /// </summary>
    /// <param name="x">Координата x.</param>
    /// <param name="y">Координата y.</param>
    /// <param name="color">Цвет.</param>
    /// <param name="boardState">Доска.</param>
    /// <param name="createGameObject">Создать ли GameObject.</param>
    public Rook(int x, int y, string color, BoardState boardState, bool createGameObject = false)
        : base(x, y, color, boardState)
    {
        if (createGameObject)
        {
            this.LoadGameObject($"{color}_rook");
        }
    }

    /// <inheritdoc/>
    public override List<(int, int)> GetMoveCells()
    {
        this.TempLegalMoveCells.Clear();
        this.TestDirection(0, 1);
        this.TestDirection(1, 0);
        this.TestDirection(0, -1);
        this.TestDirection(-1, 0);
        return this.TempLegalMoveCells;
    }
}

/// <summary>
/// Класс коня.
/// </summary>
public class Knight : Figure
{
    /// <summary>
    /// Констуктор класса Knight.
    /// </summary>
    /// <param name="x">Координата x.</param>
    /// <param name="y">Координата y.</param>
    /// <param name="color">Цвет.</param>
    /// <param name="boardState">Доска.</param>
    /// <param name="createGameObject">Создать ли GameObject.</param>
    public Knight(int x, int y, string color, BoardState boardState, bool createGameObject = false)
        : base(x, y, color, boardState)
    {
        if (createGameObject)
        {
            this.LoadGameObject($"{color}_knight");
        }
    }

    /// <inheritdoc/>
    public override List<(int, int)> GetMoveCells()
    {
        this.TempLegalMoveCells.Clear();
        this.TestCell(this.X - 1, this.Y + 2);
        this.TestCell(this.X + 1, this.Y + 2);
        this.TestCell(this.X + 2, this.Y - 1);
        this.TestCell(this.X + 2, this.Y + 1);
        this.TestCell(this.X - 1, this.Y - 2);
        this.TestCell(this.X + 1, this.Y - 2);
        this.TestCell(this.X - 2, this.Y + 1);
        this.TestCell(this.X - 2, this.Y - 1);
        return this.TempLegalMoveCells;
    }
}

/// <summary>
/// Класс слона.
/// </summary>
public class Bishop : Figure
{
    /// <summary>
    /// Констуктор класса Bishop.
    /// </summary>
    /// <param name="x">Координата x.</param>
    /// <param name="y">Координата y.</param>
    /// <param name="color">Цвет.</param>
    /// <param name="boardState">Доска.</param>
    /// <param name="createGameObject">Создать ли GameObject.</param>
    public Bishop(int x, int y, string color, BoardState boardState, bool createGameObject = false)
        : base(x, y, color, boardState)
    {
        if (createGameObject)
        {
            this.LoadGameObject($"{color}_bishop");
        }
    }

    /// <inheritdoc/>
    public override List<(int, int)> GetMoveCells()
    {
        this.TempLegalMoveCells.Clear();
        this.TestDirection(1, 1);
        this.TestDirection(1, -1);
        this.TestDirection(-1, -1);
        this.TestDirection(-1, 1);
        return this.TempLegalMoveCells;
    }
}

/// <summary>
/// Класс короля.
/// </summary>
public class King : Figure
{
    /// <summary>
    /// Констуктор класса King.
    /// </summary>
    /// <param name="x">Координата x.</param>
    /// <param name="y">Координата y.</param>
    /// <param name="color">Цвет.</param>
    /// <param name="boardState">Доска.</param>
    /// <param name="createGameObject">Создать ли GameObject.</param>
    public King(int x, int y, string color, BoardState boardState, bool createGameObject = false)
        : base(x, y, color, boardState)
    {
        if (createGameObject)
        {
            this.LoadGameObject($"{color}_king");
        }
    }

    /// <inheritdoc/>
    public override List<(int, int)> GetMoveCells()
    {
        this.TempLegalMoveCells.Clear();
        this.TestCell(this.X - 1, this.Y - 1);
        this.TestCell(this.X - 1, this.Y + 0);
        this.TestCell(this.X - 1, this.Y + 1);
        this.TestCell(this.X + 0, this.Y + 1);
        this.TestCell(this.X + 1, this.Y + 1);
        this.TestCell(this.X + 1, this.Y + 0);
        this.TestCell(this.X + 1, this.Y - 1);
        this.TestCell(this.X + 0, this.Y - 1);
        return this.TempLegalMoveCells;
    }
}

/// <summary>
/// Класс ферзя.
/// </summary>
public class Queen : Figure
{
    /// <summary>
    /// Констуктор класса Queen.
    /// </summary>
    /// <param name="x">Координата x.</param>
    /// <param name="y">Координата y.</param>
    /// <param name="color">Цвет.</param>
    /// <param name="boardState">Доска.</param>
    /// <param name="createGameObject">Создать ли GameObject.</param>
    public Queen(int x, int y, string color, BoardState boardState, bool createGameObject = false)
        : base(x, y, color, boardState)
    {
        if (createGameObject)
        {
            this.LoadGameObject($"{color}_queen");
        }
    }

    /// <inheritdoc/>
    public override List<(int, int)> GetMoveCells()
    {
        this.TempLegalMoveCells.Clear();
        this.TestDirection(0, 1);
        this.TestDirection(1, 0);
        this.TestDirection(0, -1);
        this.TestDirection(-1, 0);
        this.TestDirection(1, 1);
        this.TestDirection(1, -1);
        this.TestDirection(-1, -1);
        this.TestDirection(-1, 1);
        return this.TempLegalMoveCells;
    }
}

/// <summary>
/// Класс доски, на которой находятся фигуры.
/// </summary>
public class BoardState
{
    private readonly Figure[,] board = new Figure[8, 8];

    /// <summary>
    /// Конструктор класса BoardState.
    /// </summary>
    public BoardState()
    {
        // Белые пешки
        for (int x = 0; x < 8; x++)
        {
            new Pawn(x, 1, "white", this, true);
        }

        // Черные пешки
        for (int x = 0; x < 8; x++)
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

    /// <summary>
    /// Проверяет, находятся ли координаты в пределах доски.
    /// </summary>
    /// <param name="x">Координата x.</param>
    /// <param name="y">Координата y.</param>
    /// <returns>Находится ли координата в пределах доски.</returns>
    public static bool CoordinatesInBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    /// <summary>
    /// Получает фигуру, находящуюся в определенной клетке, или null, если там ничего нет.
    /// </summary>
    /// <param name="x">Координата x клетки.</param>
    /// <param name="y">Координата y клетки.</param>
    /// <returns>Фигура, находящаяся в клетке, или null.</returns>
    public Figure GetFigureAtCell(int x, int y)
    {
        return CoordinatesInBounds(x, y) ? this.board[x, y] : null;
    }

    /// <summary>
    /// Ставит (или убирает) фигуру в клетку.
    /// </summary>
    /// <param name="x">Координата x клетки.</param>
    /// <param name="y">Координата y клетки.</param>
    /// <param name="figure">Фигура, которую нужно поставить в клетку (или null).</param>
    public void SetFigureAtCell(int x, int y, Figure figure)
    {
        if (!CoordinatesInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException("Координаты за пределами доски");
        }
        this.board[x, y] = figure;
    }
}
