using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// Ход фигуры
/// </summary>
public class FigureMove: IEquatable<FigureMove>
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

    public bool Equals(FigureMove other)
    {
        if(other is null)
        {
            return false;
        }
        return from == other.from && to == other.to;
    }
    public override bool Equals(object obj) => Equals(obj as FigureMove);
    public override int GetHashCode() => (from, to).GetHashCode();
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
        : base(from, to, notMarkedAsIllegalRightAway)
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

    public static bool operator !=(Vector2Int one, Vector2Int another)
    {
        return !(one == another);
    }

    public static Vector2Int operator +(Vector2Int one, Vector2Int another)
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
    public Action deletedCallback;
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
    /// Определяет, можно ли фигура совершить ход в данную клетку (без учета состояния шаха). Если можно, то ход записвается во временный список.
    /// </summary>
    /// <param name="freeMove">Ход в свободную клетку.</param>
    /// <param name="takePieces">Ход со взятием фигуры.</param>
    public void TestCell(List<FigureMove> tempList, int x, int y, bool freeMove = true, bool takePieces = true)
    {
        Figure figureAtCell = boardState.GetFigureAtCell(x, y);
        bool inBounds = BoardState.CoordinatesInBounds(x, y);
        bool canFreeMove = freeMove && figureAtCell == null;
        bool canTakePiece = takePieces && figureAtCell != null && figureAtCell.color.Equals(GetEnemyColor());
        if((canFreeMove || canTakePiece) && inBounds)
        {
            tempList.Add(new FigureMove(Pos, new Vector2Int(x, y), passedFirstCheck: true));
        }
    }

    /// <summary>
    /// Проверяет, какие ходы фигура может совершить в определенном направлении, ходы записваются во временный список.
    /// </summary>
    public void TestDirection(List<FigureMove> tempList, int directionX, int directionY)
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
            TestCell(tempList, cellX, cellY, takePieces: true);
            bool ranIntoAFigure = boardState.GetFigureAtCell(cellX, cellY) != null;
            bool ranOutOfBounds = !BoardState.CoordinatesInBounds(cellX, cellY);
            if(ranIntoAFigure || ranOutOfBounds)
            {
                break;
            }
        }
    }

}
