using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;

/// <summary>
/// Ход фигуры
/// </summary>
public class FigureMove
{
    /// <summary>
    /// Начальная позиция фигуры
    /// </summary>
    public Vector2Int from;
    /// <summary>
    /// Конечная позиция фигуры
    /// </summary>
    public Vector2Int to;
    /// <summary>
    /// Некоторые ходы могут отмечаться как запрещенные сразу в методе Move
    /// </summary>
    public bool passedFirstCheck;

    public FigureMove(Vector2Int from, Vector2Int to, bool passedFirstCheck)
    {
        this.from = from;
        this.to = to;
        this.passedFirstCheck = passedFirstCheck;
    }
}

/// <summary>
/// Рокировка
/// </summary>
public class CastlingMove : FigureMove
{
    /// <summary>
    /// Начальная позиция ладьи
    /// </summary>
    public Vector2Int rookFrom;
    /// <summary>
    /// Конечная позиция ладьи
    /// </summary>
    public Vector2Int rookTo;

    public CastlingMove(Vector2Int from, Vector2Int to, Vector2Int rookFrom, Vector2Int rookTo, bool notMarkedAsIllegalRightAway) 
        :base(from, to, notMarkedAsIllegalRightAway)
    {
        this.rookFrom = rookFrom;
        this.rookTo = rookTo;
    }
}

/// <summary>
/// Аналог класса с тем же названием из Unity
/// </summary>
public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return $"({x}, {y})";
    }

    public static bool operator ==(Vector2Int one, Vector2Int another)
    {
        return (one.x == another.x) && (one.y == another.y);
    }

    public static bool operator!= (Vector2Int one, Vector2Int another)
    {
        return !(one == another);
    }

    public static Vector2Int operator+ (Vector2Int one, Vector2Int another)
    {
        return new Vector2Int(one.x + another.x, one.y + another.y);
    }
}

/// <summary>
/// Абстрактный класс фигуры
/// </summary>
[DataContract(Name = "Figure")]
[KnownType(typeof(Pawn))]
[KnownType(typeof(Rook))]
[KnownType(typeof(Knight))]
[KnownType(typeof(Bishop))]
[KnownType(typeof(King))]
[KnownType(typeof(Queen))]
public abstract class Figure
{
    /// <summary>
    /// Возможные цвета фигур
    /// </summary>
    public enum FigureColor
    {
        white,
        black
    }
    /// <summary>
    /// Тип для deletedCallback
    /// </summary>
    public delegate void DeletedCallbackDelegate();
    /// <summary>
    /// Тип для movedCallback
    /// </summary>
    public delegate void MovedCallbackDelegate(Vector2Int newPos);

    /// <summary>
    /// Позиция x фигуры на доске
    /// </summary>
    [DataMember]
    public int x;
    /// <summary>
    /// Позиция y фигуры на доске
    /// </summary>
    [DataMember]
    public int y;
    /// <summary>
    /// Позиция фигуры на доске
    /// </summary>
    public Vector2Int Pos {
        get => new Vector2Int(x, y);
        set {
            x = value.x;
            y = value.y;
        }
    }
    /// <summary>
    /// Вызывется при удалении фигуры с доски (например, ее взяли)
    /// </summary>
    public DeletedCallbackDelegate deletedCallback;
    /// <summary>
    /// Вызывается когда фигура была передвинута
    /// </summary>
    public MovedCallbackDelegate movedCallback;
    /// <summary>
    /// Цвет фигуры (белый или черный)
    /// </summary>
    [DataMember]
    public FigureColor color;
    /// <summary>
    /// Доска на которой находится фигура
    /// </summary>
    [DataMember]
    public BoardState boardState;
    /// <summary>
    /// Количество ходов, совершенное фигурой с начала партии
    /// </summary>
    [DataMember]
    public int moveCount = 0;
    /// <summary>
    /// Временный список, используется функциями GetAllMoves фигур
    /// </summary>
    protected List<FigureMove> tempMoveList = new List<FigureMove>();

    /// <summary>
    /// Получить все ходы фигуры (без учета ходов приводящих к шаху)
    /// </summary>
    /// <param name="special">Включать рокировку или нет.
    /// Вызов GetAllMoves при проверке рокировки рекурсивно вызывают GetAllMoves.
    /// Этот параметр позволяет избежать хвостовой рекурсии.</param>
    /// <returns>Список возможных ходов фигуры, включая ходы приводящие к шаху.</returns>
    public abstract List<FigureMove> GetAllMoves(bool special);

    public Figure(int x, int y, FigureColor color, BoardState boardState)
    {
        if(!BoardState.CoordinatesInBounds(x, y))
        {
            throw new ArgumentOutOfRangeException("Нельзя создать фигуру за пределами доски");
        }
        this.x = x;
        this.y = y;
        this.color = color;
        this.boardState = boardState ?? throw new ArgumentNullException("boardState");
    }

    /// <summary>
    /// Создает копию фигуры.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находится копия.</param>
    /// <returns>Копия фигуры.</returns>
    public Figure Copy(BoardState boardState)
    {
        if(boardState == null)
        {
            throw new ArgumentNullException("boardState");
        }
        Figure copy = (Figure)GetType().GetConstructors()[0].Invoke(new object[] { x, y, color, boardState });
        copy.moveCount = moveCount;
        return copy;
    }

    /// <summary>
    /// Получает все разрешенные ходы данной фигуры. Не включает ходы приводящие к шаху.
    /// </summary>
    /// <returns>Список разрешенных ходов данной фигуры.</returns>
    public List<FigureMove> GetLegalMoves()
    {
        List<FigureMove> moves = boardState.GetLegalMoves().FindAll((move) => move.from == Pos);
        return moves;
    }

    /// <summary>
    /// Получает список клеток находящихся под боем данной фигуры
    /// </summary>
    /// <returns></returns>
    public virtual List<Vector2Int> GetCellsUnderAttack()
    {
        List<FigureMove> moves = GetAllMoves(special: false);
        List<Vector2Int> destinations = BoardState.GetMoveDestinations(moves);
        return destinations;
    }

    /// <summary>
    /// Двигает фигуру в другую клетку.
    /// </summary>
    /// <param name="takeFigure">Можно ли брать вражеские фигуры.</param>
    protected static void MoveFigure(Figure figure, Vector2Int newPos, bool takeFigure)
    {
        if(!BoardState.CoordinatesInBounds(newPos))
        {
            throw new ArgumentOutOfRangeException("Нельзя передвинуть фигуру за пределы доски");
        }
        // Если в клетке уже есть фигура
        Figure figureAtCell = figure.boardState.GetFigureAtCell(newPos);
        if(figureAtCell != null)
        {
            if(!takeFigure)
            {
                throw new InvalidOperationException("Нельзя передвинуть фигуру: нельзя брать фигуры");
            }
            if(figureAtCell.color == figure.color)
            {
                throw new InvalidOperationException("Нельзя передвинуть фигуру: в клетке уже стоит своя фигура");
            }
            figureAtCell.Delete();
        }
        // Изменяем параметры фигуры
        Vector2Int oldPos = figure.Pos;
        figure.Pos = newPos;
        figure.movedCallback?.Invoke(newPos);
        figure.moveCount++;
    }

    /// <summary>
    /// Совершить ход фигурой. Специальные ходы (рокировка, взятие на проходе) описыватся в override методах дочерних классов.
    /// </summary>
    /// <param name="updateLegalMoves">Обновить список разрешенных ходов.</param>
    public virtual void ExecuteMove(FigureMove move)
    {
        if(color != boardState.turnColor)
        {
            throw new InvalidOperationException($"Невозможно совершить ход: сейчас ходит {boardState.turnColor}, а цвет фигуры {color}");
        }
        if(move.from != Pos)
        {
            throw new ArgumentException($"Начальная клетка FigureMove {move.from} не совпадает с позицией фигуры {Pos}");
        }
        MoveFigure(this, move.to, takeFigure: true);
        boardState.turnColor = InvertColor(boardState.turnColor);
    }

    /// <summary>
    /// Удаляет фигуру с доски. Также вызывет deletedCallback.
    /// </summary>
    public void Delete()
    {
        boardState.figures.Remove(this);
        deletedCallback?.Invoke();
    }

    /// <summary>
    /// Инвертировать цвет.
    /// </summary>
    /// <returns>black если white, white если black.</returns>
    public static FigureColor InvertColor(FigureColor color)
    {
        return color == FigureColor.black ? FigureColor.white : FigureColor.black;
    }

    /// <summary>
    /// Получить цвет вражеских фигур.
    /// </summary>
    public FigureColor GetEnemyColor()
    {
        return InvertColor(color);
    }

    /// <summary>
    /// Определяет, можно ли фигура совершить ход в данную клетку (без учета состояния шаха), если можно, то ход записвается во временный список.
    /// </summary>
    /// <param name="freeMove">Ход в свободную клетку.</param>
    /// <param name="takePieces">Ход со взятием фигуры.</param>
    public void TestCell(int x, int y, bool freeMove = true, bool takePieces = true)
    {
        Figure figureAtCell = boardState.GetFigureAtCell(x, y);
        bool inBounds = BoardState.CoordinatesInBounds(x, y);
        bool canFreeMove = freeMove && figureAtCell == null;
        bool canTakePiece = takePieces && figureAtCell != null && figureAtCell.color.Equals(GetEnemyColor());
        if((canFreeMove || canTakePiece) && inBounds)
        {
            tempMoveList.Add(new FigureMove(Pos, new Vector2Int(x, y), passedFirstCheck: true));
        }
    }

    /// <summary>
    /// Проверяет, какие ходы фигура может совершить в определенном направлении, ходы записваются во временный список.
    /// </summary>
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
}

/// <summary>
/// Класс пешки.
/// </summary>
[DataContract(Name = "Pawn")]
public class Pawn: Figure
{
    /// <summary>
    /// Конструктор пешки.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находиться фигура.</param>
    public Pawn(int x, int y, FigureColor color, BoardState boardState): base(x, y, color, boardState) {}

    /// <summary>
    /// Получает все ходы пешки (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае пешки не используется.</param>
    /// <returns>Все ходы пешки (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        tempMoveList.Clear();
        int direction = color == FigureColor.white ? 1 : -1;

        // Ход на 1 клетку вперед
        TestCell(x, y + direction, takePieces: false);
        // Ход на 2 клетки вперед
        if(moveCount == 0 && boardState.GetFigureAtCell(x, y + direction) == null)
        {
            TestCell(x, y + direction * 2, takePieces: false);
        }
        // Ходы по диагонали
        TestCell(x - 1, y + direction, freeMove: false);
        TestCell(x + 1, y + direction, freeMove: false);

        return tempMoveList;
    }

    public override List<Vector2Int> GetCellsUnderAttack()
    {
        int direction = color == FigureColor.white ? 1 : -1;
        Vector2Int left = Pos + new Vector2Int(-1, direction);
        Vector2Int right = Pos + new Vector2Int(1, direction);
        return new List<Vector2Int>() { left, right };
    }

    /// <summary>
    /// Двигает пешку в другую клетку.
    /// </summary>
    public override void ExecuteMove(FigureMove move)
    {
        // Взятие на проходе
        bool longDistanceY = Math.Abs(move.to.y - y) > 1;
        if(longDistanceY)
        {
            int direction = color == FigureColor.white ? 1 : -1;
            Figure left = boardState.GetFigureAtCell(Pos + new Vector2Int(-1, direction));
            Figure right = boardState.GetFigureAtCell(Pos + new Vector2Int(1, direction));
            if(left != null && left.GetType() == typeof(Pawn) && left.color == InvertColor(color))
            {
                left.Delete();
            }
            if(right != null && right.GetType() == typeof(Pawn) && right.color == InvertColor(color))
            {
                right.Delete();
            }
        }
        // Вызов базового метода ExecuteMove
        base.ExecuteMove(move);
    }
}

/// <summary>
/// Класс ладьи.
/// </summary>
[DataContract(Name = "Rook")]
public class Rook : Figure
{
    /// <summary>
    /// Конструктор ладьи.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находиться ладья.</param>
    public Rook(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    /// <summary>
    /// Получает все ходы ладьи (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае ладьи не используется.</param>
    /// <returns>Все ходы ладьи (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        tempMoveList.Clear();
        TestDirection(0, 1);
        TestDirection(1, 0);
        TestDirection(0, -1);
        TestDirection(-1, 0);
        return tempMoveList;
    }
}

/// <summary>
/// Класс коня.
/// </summary>
[DataContract(Name = "Knight")]
public class Knight : Figure
{
    /// <summary>
    /// Конструктор коня.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находиться конь.</param>
    public Knight(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    /// <summary>
    /// Получает все ходы коня (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае коня не используется.</param>
    /// <returns>Все ходы коня (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        tempMoveList.Clear();
        TestCell(x - 1, y + 2);
        TestCell(x + 1, y + 2);
        TestCell(x + 2, y - 1);
        TestCell(x + 2, y + 1);
        TestCell(x - 1, y - 2);
        TestCell(x + 1, y - 2);
        TestCell(x - 2, y + 1);
        TestCell(x - 2, y - 1);
        return tempMoveList;
    }
}

/// <summary>
/// Класс слона.
/// </summary>
[DataContract(Name = "Bishop")]
public class Bishop : Figure
{
    /// <summary>
    /// Конструктор слона.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находиться конь.</param>
    public Bishop(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    /// <summary>
    /// Получает все ходы слона (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае слона не используется.</param>
    /// <returns>Все ходы слона (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        tempMoveList.Clear();
        TestDirection(1, 1);
        TestDirection(1, -1);
        TestDirection(-1, -1);
        TestDirection(-1, 1);
        return tempMoveList;
    }

}

/// <summary>
/// Класс короля.
/// </summary>
[DataContract(Name = "King")]
public class King : Figure
{
    /// <summary>
    /// Конструктор короля.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находиться король.</param>
    public King(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    /// <summary>
    /// Сторона рокировки.
    /// </summary>
    public enum CastlingSide
    {
        /// <summary>
        /// Со стороны королевы.
        /// </summary>
        queenside,
        /// <summary>
        /// Со стороны короля.
        /// </summary>
        kingside
    }

    /// <summary>
    /// Проверяет, возможна ли рокировка с определнной стороны. Ход записывается во временный список.
    /// </summary>
    /// <param name="side">Сторона рокировки.</param>
    public void TestCastling(CastlingSide side)
    {
        // Определяем клетки между королем и ладьей (они должны быть свободны).
        List<Vector2Int> cellsBetweenKingAndRook = new List<Vector2Int>();
        // Определяем клетки, которые проходит король при рокировке (они должны быть не по боем вражеских фигур).
        List<Vector2Int> kingPassesCells = new List<Vector2Int>();
        Vector2Int rookPos;
        Vector2Int rookNewPos;
        if(side == CastlingSide.queenside)
        {
            cellsBetweenKingAndRook.Add(new Vector2Int(1, y));
            cellsBetweenKingAndRook.Add(new Vector2Int(2, y));
            cellsBetweenKingAndRook.Add(new Vector2Int(3, y));
            kingPassesCells.Add(new Vector2Int(2, y));
            kingPassesCells.Add(new Vector2Int(3, y));
            // Ладья будет со стороны королевы
            rookPos = new Vector2Int(0, y);
            rookNewPos = new Vector2Int(3, y);
        }
        else
        {
            cellsBetweenKingAndRook.Add(new Vector2Int(5, y));
            cellsBetweenKingAndRook.Add(new Vector2Int(6, y));
            kingPassesCells.Add(new Vector2Int(5, y));
            kingPassesCells.Add(new Vector2Int(6, y));
            // Ладья будет со стороны короля
            rookPos = new Vector2Int(7, y);
            rookNewPos = new Vector2Int(5, y);
        }
        // Ищем ладью
        Figure figureInTheCorner = boardState.GetFigureAtCell(rookPos);
        bool rookInPlace = figureInTheCorner != null && figureInTheCorner.GetType() == typeof(Rook) && figureInTheCorner.color == color;
        if(!rookInPlace)
        {
            return;
        }
        Rook rook = (Rook)figureInTheCorner;
        // Фигуры не двигались с начала партии
        bool figuresNotMoved = moveCount == 0 && rook.moveCount == 0;
        // Клетки меджу ними свободны
        bool cellsBetweenAreFree = cellsBetweenKingAndRook.TrueForAll((cell) => boardState.GetFigureAtCell(cell) == null);
        // Клетки между ними не под боем
        bool kingCellsAreUnderAttack = boardState.AnyCellIsUnderAttack(kingPassesCells, InvertColor(color));
        // Король не под шахом
        bool kingIsUnderAttack = boardState.DetectCheck(color);
        if(figuresNotMoved && cellsBetweenAreFree)
        {
            int newX = side == CastlingSide.queenside ? 2 : 6;
            tempMoveList.Add(
                new CastlingMove(
                    from: Pos,
                    to: new Vector2Int(newX, y),
                    rookFrom: rookPos,
                    rookTo: rookNewPos,
                    notMarkedAsIllegalRightAway: !kingCellsAreUnderAttack && !kingIsUnderAttack
                )
            );
        }
    }

    /// <summary>
    /// Получает все ходы короля (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">Включать ли рокировку.</param>
    /// <returns>Все ходы короля (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        tempMoveList.Clear();
        TestCell(x - 1, y - 1);
        TestCell(x - 1, y + 0);
        TestCell(x - 1, y + 1);
        TestCell(x + 0, y + 1);
        TestCell(x + 1, y + 1);
        TestCell(x + 1, y + 0);
        TestCell(x + 1, y - 1);
        TestCell(x + 0, y - 1);
        if(special)
        {
            TestCastling(CastlingSide.queenside);
            TestCastling(CastlingSide.kingside);
        }
        return tempMoveList;
    }

    /// <summary>
    /// Двигает короля в другую клетку.
    /// </summary>
    public override void ExecuteMove(FigureMove move)
    {
        if(move.GetType() == typeof(CastlingMove))
        {
            CastlingMove castlingMove = (CastlingMove)move;
            // Ищем ладью
            Figure rook = boardState.GetFigureAtCell(castlingMove.rookFrom);
            if(rook == null || rook.GetType() != typeof(Rook))
            {
                throw new InvalidOperationException("Не найдена ладья для рокировки");
            }
            // Двигаем ладью
            MoveFigure(rook, castlingMove.rookTo, takeFigure: false);
        }
        // Вызов базового метода Move
        base.ExecuteMove(move);
    }

}

/// <summary>
/// Класс ферзя.
/// </summary>
[DataContract(Name = "Queen")]
public class Queen : Figure
{
    /// <summary>
    /// Конструктор ферзя.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находиться ферзь.</param>
    public Queen(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    /// <summary>
    /// Получает все ходы ферзя (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае ферзя не используется.</param>
    /// <returns>Все ходы ферзя (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        tempMoveList.Clear();
        TestDirection(0, 1);
        TestDirection(1, 0);
        TestDirection(0, -1);
        TestDirection(-1, 0);
        TestDirection(1, 1);
        TestDirection(1, -1);
        TestDirection(-1, -1);
        TestDirection(-1, 1);
        return tempMoveList;
    }
}

/// <summary>
/// Класс доски.
/// </summary>
[DataContract(Name = "BoardState", IsReference = true)]
public class BoardState
{
    /// <summary>
    /// Список фигур.
    /// </summary>
    [DataMember]
    public readonly List<Figure> figures = new List<Figure>();
    /// <summary>
    /// Цвет фигур, которые сейчас ходят.
    /// </summary>
    [DataMember]
    public Figure.FigureColor turnColor;

    /// <summary>
    /// Конструктор доски. Заполяет доску фигурами как в начале партии.
    /// </summary>
    public BoardState()
    {

        // Сокращенные названия
        Figure.FigureColor white = Figure.FigureColor.white;
        Figure.FigureColor black = Figure.FigureColor.black;

        // Белые пешки
        for(int x = 0; x < 8; x++)
        {
            figures.Add(new Pawn(x, 1, white, this));
        }
        // Черные пешки
        for(int x = 0; x < 8; x++)
        {
            figures.Add(new Pawn(x, 6, black, this));
        }
        // Ладьи
        figures.Add(new Rook(0, 0, white, this));
        figures.Add(new Rook(7, 0, white, this));
        figures.Add(new Rook(0, 7, black, this));
        figures.Add(new Rook(7, 7, black, this));
        // Кони
        figures.Add(new Knight(1, 0, white, this));
        figures.Add(new Knight(6, 0, white, this));
        figures.Add(new Knight(1, 7, black, this));
        figures.Add(new Knight(6, 7, black, this));
        // Слоны
        figures.Add(new Bishop(2, 0, white, this));
        figures.Add(new Bishop(5, 0, white, this));
        figures.Add(new Bishop(2, 7, black, this));
        figures.Add(new Bishop(5, 7, black, this));
        // Короли
        figures.Add(new King(4, 0, white, this));
        figures.Add(new King(4, 7, black, this));
        // Ферзи
        figures.Add(new Queen(3, 0, white, this));
        figures.Add(new Queen(3, 7, black, this));

        // Белые ходят первыми
        turnColor = Figure.FigureColor.white;
    }

    /// <summary>
    /// Копирующий конструктор. Фигуры также копируются.
    /// </summary>
    public BoardState(BoardState original)
    {
        turnColor = original.turnColor;
        figures = new List<Figure>();
        foreach(Figure figure in original.figures)
        {
            figures.Add(figure.Copy(this));
        }
    }

    /// <summary>
    /// Определяет, находятся ли координаты в пределах доски.
    /// </summary>
    /// <returns>Находятся ли координаты в пределах доски.</returns>
    public static bool CoordinatesInBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    /// <summary>
    /// Определяет, находятся ли координаты в пределах доски.
    /// </summary>
    /// <returns>Находятся ли координаты в пределах доски.</returns>
    public static bool CoordinatesInBounds(Vector2Int coords)
    {
        return CoordinatesInBounds(coords.x, coords.y);
    }

    /// <summary>
    /// Получает список клеток назначения из списка ходов.
    /// </summary>
    /// <param name="moves">Список ходов.</param>
    /// <returns>Список клеток назначения (без повторяющихся эелементов).</returns>
    public static List<Vector2Int> GetMoveDestinations(List<FigureMove> moves)
    {
        return (from move in moves select move.to).Distinct().ToList();
    }

    /// <summary>
    /// Получает фигуру, находящуюся в определенной клетке.
    /// </summary>
    /// <returns>Фигура стоящая в клетке или null если там ничего нет.</returns>
    public Figure GetFigureAtCell(int x, int y)
    {
        return GetFigureAtCell(new Vector2Int(x, y));
    }

    /// <summary>
    /// Получает фигуру, находящуюся в определенной клетке.
    /// </summary>
    /// <returns>Фигура стоящая в клетке или null если там ничего нет.</returns>
    public Figure GetFigureAtCell(Vector2Int cell)
    {
        List<Figure> figuresAtCell = figures.FindAll(figure => figure.Pos == cell);
        if(figuresAtCell.Count > 1)
        {
            string figureStr = "";
            foreach(Figure figure in figuresAtCell)
            {
                figureStr += " " + figure.GetType().Name;
            }

            throw new InvalidOperationException($"В клетке {cell} находятся {figuresAtCell.Count} фигуры: {figureStr}");
        }
        if(figuresAtCell.Count == 0)
        {
            return null;
        }
        return figuresAtCell[0];
    }

    /// <summary>
    /// Получает все фигуры на доске.
    /// </summary>
    /// <returns>Список фигур на доске.</returns>
    public List<Figure> GetFigures()
    {
        return figures;
    }

    /// <summary>
    /// Определяет находится ли клетка под боем фигур определенного цвета.
    /// </summary>
    /// <param name="color">Клетка находится под боем фигур этого цвета.</param>
    public bool CellIsUnderAttack(Vector2Int cell, Figure.FigureColor color)
    {
        return GetAllCellsUnderAttackByColor(color).Contains(cell);
    }

    /// <summary>
    /// Определяет, находится ли любая из клеток из списка под боем фигур определенного цвета.
    /// </summary>
    /// <param name="color">Клетки находятся под боем фигур этого цвета.</param>
    public bool AnyCellIsUnderAttack(List<Vector2Int> cells, Figure.FigureColor color)
    {
        return cells.Intersect(GetAllCellsUnderAttackByColor(color)).Count() > 0;
    }

    /// <summary>
    /// Определяет, поставлен ли королю опрелеленного цвета шах.
    /// </summary>
    /// <param name="color">Королю какого цвета поствлен шах.</param>
    public bool DetectCheck(Figure.FigureColor color)
    {
        Figure.FigureColor enemyColor = Figure.InvertColor(color);
        List<Figure> ownFigures = GetFiguresByColor(color);
        King king = (King)(from Figure figure in ownFigures where figure.GetType() == typeof(King) select figure).First();
        return CellIsUnderAttack(king.Pos, enemyColor);
    }

    /// <summary>
    /// Определяет, поставлен ли королю того цвета который сейчас ходит мат.
    /// </summary>
    public bool DetectMate()
    {
        // Определение мата
        return GetLegalMoves().Count == 0;
    }

    /// <summary>
    /// Получает список фигур определенного цвета.
    /// </summary>
    public List<Figure> GetFiguresByColor(Figure.FigureColor color)
    {
        List<Figure> allFigures = GetFigures();
        List<Figure> colorFigures = (from Figure figure in allFigures where figure.color == color select figure).ToList();
        return colorFigures;
    }

    /// <summary>
    /// Получает список ходов (в том числе приводящих к шаху) фигур определенного цвета.
    /// </summary>
    public List<FigureMove> GetMovesByColor(Figure.FigureColor color, bool special)
    {
        // Получаем фигуры
        List<Figure> figures = GetFiguresByColor(color);
        // Получаем список ходов
        List<FigureMove> colorMoves = figures.SelectMany(figure => figure.GetAllMoves(special)).ToList();
        return colorMoves;
    }

    /// <summary>
    /// Получает все клетки, находящиеся под боем фигур определенного цвета
    /// </summary>
    public List<Vector2Int> GetAllCellsUnderAttackByColor(Figure.FigureColor color)
    {
        // Получаем фигуры
        List<Figure> colorFigures = GetFiguresByColor(color);
        // Получаем список клеток под боем
        List<Vector2Int> allCellsUnderAttack = colorFigures.SelectMany(figure => figure.GetCellsUnderAttack()).ToList();
        return allCellsUnderAttack;
    }

    /// <summary>
    /// Обновляет список разрешенных (не приводящих к шаху) ходов фигур того цвета, который сейчас ходит.
    /// </summary>
    /// <param name="color"></param>
    public List<FigureMove> GetLegalMoves()
    {
        List<FigureMove> legalMoves = new List<FigureMove>();
        // Получаем список возможных ходов
        List<FigureMove> ownMoves = GetMovesByColor(turnColor, special: true);
        // Пытаемся сделать ход
        foreach(FigureMove ownMove in ownMoves)
        {
            // Если ход уже отмечен как запрещенный, его можно не проверять
            if(!ownMove.passedFirstCheck)
            {
                continue;
            }
            BoardState virtualBoard = new BoardState(this);
            Figure figure = virtualBoard.GetFigureAtCell(ownMove.from);
            figure.ExecuteMove(ownMove);
            if(!virtualBoard.DetectCheck(turnColor))
            {
                legalMoves.Add(ownMove);
            }
        }
        return legalMoves;
    }

    /// <summary>
    /// Совершить ход.
    /// </summary>
    public void ExecuteMove(FigureMove move)
    {
        Figure figure = GetFigureAtCell(move.from);
        if(figure == null)
        {
            throw new InvalidOperationException("Невозможно совершить ход: не найдена фигура");
        }
        figure.ExecuteMove(move);
    }

    /// <summary>
    /// Сохранить в файл.
    /// </summary>
    /// <param name="filename"></param>
    public void Serialize(string filename)
    {
        var settings = new XmlWriterSettings {
            Indent = true,
            IndentChars = "    "
        };
        XmlWriter writer = XmlWriter.Create(filename, settings);
        DataContractSerializer ser = new DataContractSerializer(typeof(BoardState));
        ser.WriteObject(writer, this);
        writer.Close();
    }

}
