using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public struct FigureMove
{
    public Vector2Int from;
    public Vector2Int to;
    public bool notMarkedAsIllegalRightAway;

    public FigureMove(Vector2Int from, Vector2Int to, bool notMarkedAsIllegalRightAway)
    {
        this.from = from;
        this.to = to;
        this.notMarkedAsIllegalRightAway = notMarkedAsIllegalRightAway;
    }
}

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

public abstract class Figure
{

    public enum FigureColor
    {
        white,
        black
    }
    public delegate void VoidDelegate();
    public delegate void IntIntVoidDelegate(int newX, int newY);

    public int x;
    public int y;
    public Vector2Int Pos {
        get => new Vector2Int(x, y);
        set {
            x = value.x;
            y = value.y;
        }
    }
    public VoidDelegate deletedCallback;
    public IntIntVoidDelegate movedCallback;
    public FigureColor color;
    public BoardState boardState;
    public int moveCount = 0;

    protected List<FigureMove> tempMoveList = new List<FigureMove>();


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
        boardState.SetFigureAtCell(x, y, this);
    }

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

    public List<FigureMove> GetLegalMoves()
    {
        List<FigureMove> allMoves = GetAllMoves(special: true);
        List<FigureMove> moves = boardState.legalMoves.FindAll((move) => move.from == Pos && move.notMarkedAsIllegalRightAway);
        return moves;
    }

    protected static void MoveFigure(Figure figure, int newX, int newY, bool takeFigure)
    {
        if(!BoardState.CoordinatesInBounds(newX, newY))
        {
            throw new ArgumentOutOfRangeException("Нельзя передвинуть фигуру за пределы доски");
        }
        // Если в клетке уже есть фигура
        Figure figureAtCell = figure.boardState.GetFigureAtCell(newX, newY);
        if(figureAtCell != null)
        {
            if(!takeFigure)
            {
                throw new InvalidOperationException("Нельзя передвинуть фигуру: в клетке уже есть фигура");
            }
            figureAtCell.Delete();
        }
        // Изменяем параметры фигуры
        int oldX = figure.x;
        int oldY = figure.y;
        figure.x = newX;
        figure.y = newY;
        figure.movedCallback?.Invoke(newX, newY);
        figure.moveCount++;
        // Изменяем состояние доски
        figure.boardState.SetFigureAtCell(oldX, oldY, null);
        figure.boardState.SetFigureAtCell(newX, newY, figure);
    }

    protected static void MoveFigure(Figure figure, Vector2Int newPos, bool takeFigure)
    {
        MoveFigure(figure, newPos.x, newPos.y, takeFigure);
    }

    public virtual void Move(int newX, int newY)
    {
        MoveFigure(this, newX, newY, takeFigure: true);
        boardState.turnColor = InvertColor(boardState.turnColor);
    }

    public void Move(Vector2Int cell)
    {
        Move(cell.x, cell.y);
    }

    public void Delete()
    {
        deletedCallback?.Invoke();
        if(boardState.GetFigureAtCell(Pos) == null)
        {
            throw new InvalidOperationException("Возможно фигура уже была удалена");
        }
        boardState.SetFigureAtCell(Pos, null);
    }

    public static FigureColor InvertColor(FigureColor color)
    {
        return color == FigureColor.black ? FigureColor.white : FigureColor.black;
    }

    public FigureColor GetEnemyColor()
    {
        return InvertColor(color);
    }

    public void TestCell(int x, int y, bool freeMove = true, bool takePieces = true)
    {
        Figure figureAtCell = boardState.GetFigureAtCell(x, y);
        bool inBounds = BoardState.CoordinatesInBounds(x, y);
        bool canFreeMove = freeMove && figureAtCell == null;
        bool canTakePiece = takePieces && figureAtCell != null && figureAtCell.color.Equals(GetEnemyColor());
        if((canFreeMove || canTakePiece) && inBounds)
        {
            tempMoveList.Add(new FigureMove(Pos, new Vector2Int(x, y), notMarkedAsIllegalRightAway: true));
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
}

public class Pawn: Figure
{
    public Pawn(int x, int y, FigureColor color, BoardState boardState): base(x, y, color, boardState) {}

    public override List<FigureMove> GetAllMoves(bool special)
    {
        tempMoveList.Clear();
        int direction = color == FigureColor.white ? 1 : -1;

        TestCell(x, y + direction, takePieces: false);
        if(moveCount == 0 && boardState.GetFigureAtCell(x, y + direction) == null)
        {
            TestCell(x, y + direction * 2, takePieces: false);
        }
        TestCell(x - 1, y + direction, freeMove: false);
        TestCell(x + 1, y + direction, freeMove: false);

        return tempMoveList;
    }

    public override void Move(int newX, int newY)
    {
        // Взятие на проходе
        bool longDistanceY = Math.Abs(newY - y) > 1;
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
        base.Move(newX, newY);
    }

}

public class Rook : Figure
{
    public Rook(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

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

public class Knight : Figure
{
    public Knight(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

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

public class Bishop : Figure
{
    public Bishop(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

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

public class King : Figure
{
    public King(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    public enum CastlingSide
    {
        queenside,
        kingside
    }

    public void TestCastling(CastlingSide side)
    {
        // Определяем клетки между королем и ладьей
        List<Vector2Int> cellsBetweenKingAndRook = new List<Vector2Int>();
        List<Vector2Int> kingPassesCells = new List<Vector2Int>();
        Vector2Int rookPos;
        if(side == CastlingSide.queenside)
        {
            cellsBetweenKingAndRook.Add(new Vector2Int(1, y));
            cellsBetweenKingAndRook.Add(new Vector2Int(2, y));
            cellsBetweenKingAndRook.Add(new Vector2Int(3, y));
            kingPassesCells.Add(new Vector2Int(2, y));
            kingPassesCells.Add(new Vector2Int(3, y));
            rookPos = new Vector2Int(0, y);
        }
        else
        {
            cellsBetweenKingAndRook.Add(new Vector2Int(5, y));
            cellsBetweenKingAndRook.Add(new Vector2Int(6, y));
            kingPassesCells.Add(new Vector2Int(5, y));
            kingPassesCells.Add(new Vector2Int(6, y));
            rookPos = new Vector2Int(7, y);
        }
        // Ищем ладью
        Figure rook = boardState.GetFigureAtCell(rookPos);
        bool rookInPlace = rook != null && rook.GetType() == typeof(Rook) && rook.color == color;
        if(!rookInPlace)
        {
            return;
        }
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
            tempMoveList.Add(new FigureMove(Pos, new Vector2Int(newX, y), notMarkedAsIllegalRightAway: !kingCellsAreUnderAttack && !kingIsUnderAttack));
        }
    }

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

    public override void Move(int newX, int newY)
    {
        // Рокировка
        bool longDistanceX = Math.Abs(newX - x) > 1;
        if(longDistanceX)
        {
            CastlingSide side = newX < x ? CastlingSide.queenside : CastlingSide.kingside;
            Vector2Int rookPos;
            Vector2Int rookNewPos;
            if(color == FigureColor.white && side == CastlingSide.queenside)
            {
                rookPos = new Vector2Int(0, 0);
                rookNewPos = new Vector2Int(3, 0);
            }
            else if(color == FigureColor.white && side == CastlingSide.kingside)
            {
                rookPos = new Vector2Int(7, 0);
                rookNewPos = new Vector2Int(5, 0);
            }
            else if(color == FigureColor.black && side == CastlingSide.queenside)
            {
                rookPos = new Vector2Int(0, 7);
                rookNewPos = new Vector2Int(3, 7);
            }
            else if(color == FigureColor.black && side == CastlingSide.kingside)
            {
                rookPos = new Vector2Int(7, 7);
                rookNewPos = new Vector2Int(5, 7);
            }
            else
            {
                throw new Exception($"Неверные значения color или side: color:{color} side:{side}");
            }
            Figure rook = boardState.GetFigureAtCell(rookPos);
            if(rook == null)
            {
                throw new InvalidOperationException("Не найдена ладья для рокировки");
            }
            if(boardState.GetFigureAtCell(rookNewPos) != null)
            {
                throw new InvalidOperationException("Не удалось передвинуть ладью: в клетке уже есть фигура");
            }
            // Двигаем ладью
            MoveFigure(rook, rookNewPos, takeFigure: false);
        }
        base.Move(newX, newY);
    }

}

public class Queen : Figure
{
    public Queen(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

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

public class BoardState
{

    private readonly Figure[,] board = new Figure[8, 8];
    public readonly List<FigureMove> legalMoves = new List<FigureMove>();
    public Figure.FigureColor turnColor;

    public BoardState()
    {
        Figure.FigureColor white = Figure.FigureColor.white;
        Figure.FigureColor black = Figure.FigureColor.black;

        // Белые пешки
        for(int x = 0; x < 8; x++)
        {
            new Pawn(x, 1, white, this);
        }
        // Черные пешки
        for(int x = 0; x < 8; x++)
        {
            new Pawn(x, 6, black, this);
        }
        // Ладьи
        new Rook(0, 0, white, this);
        new Rook(7, 0, white, this);
        new Rook(0, 7, black, this);
        new Rook(7, 7, black, this);
        // Кони
        new Knight(1, 0, white, this);
        new Knight(6, 0, white, this);
        new Knight(1, 7, black, this);
        new Knight(6, 7, black, this);
        // Слоны
        new Bishop(2, 0, white, this);
        new Bishop(5, 0, white, this);
        new Bishop(2, 7, black, this);
        new Bishop(5, 7, black, this);
        // Короли
        new King(4, 0, white, this);
        new King(4, 7, black, this);
        // Ферзи
        new Queen(3, 0, white, this);
        new Queen(3, 7, black, this);

        turnColor = Figure.FigureColor.white;
        UpdateLegalMoves(white);
    }

    public BoardState(BoardState original)
    {
        board = new Figure[8, 8];
        legalMoves = original.legalMoves;
        turnColor = original.turnColor;
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

    public static List<Vector2Int> GetMoveDestinations(List<FigureMove> moves)
    {
        return (from move in moves select move.to).Distinct().ToList();
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

    public void SetFigureAtCell(Vector2Int pos, Figure figure)
    {
        SetFigureAtCell(pos.x, pos.y, figure);
    }

    public List<Figure> GetFigures()
    {
        return (from Figure figure in board where figure != null select figure).ToList();
    }

    public bool CellIsUnderAttack(Vector2Int cell, Figure.FigureColor color)
    {
        List<FigureMove> moves = GetMovesByColor(color, special: false);
        List<Vector2Int> destinations = GetMoveDestinations(moves);
        return destinations.Contains(cell);
    }

    public bool AnyCellIsUnderAttack(List<Vector2Int> cells, Figure.FigureColor color)
    {
        List<FigureMove> moves = GetMovesByColor(color, special: false);
        List<Vector2Int> destinations = GetMoveDestinations(moves);
        return cells.Intersect(destinations).Count() > 0;
    }

    public bool DetectCheck(Figure.FigureColor color)
    {
        Figure.FigureColor enemyColor = Figure.InvertColor(color);
        List<Figure> ownFigures = GetFiguresByColor(color);
        King king = (King)(from Figure figure in ownFigures where figure.GetType() == typeof(King) select figure).First();
        return CellIsUnderAttack(king.Pos, enemyColor);
    }

    public bool DetectMate()
    {
        // Определение мата
        return legalMoves.Count == 0;
    }

    public List<Figure> GetFiguresByColor(Figure.FigureColor color)
    {
        List<Figure> allFigures = GetFigures();
        List<Figure> colorFigures = (from Figure figure in allFigures where figure.color == color select figure).ToList();
        return colorFigures;
    }

    public List<FigureMove> GetMovesByColor(Figure.FigureColor color, bool special)
    {
        // Получаем фигуры
        List<Figure> figures = GetFiguresByColor(color);
        // Получаем список ходов
        List<FigureMove> colorMoves = new List<FigureMove>();
        foreach(Figure figure in figures)
        {
            List<FigureMove> moves = figure.GetAllMoves(special);
            colorMoves = colorMoves.Concat(moves).ToList();
        }
        return colorMoves;
    }

    public void UpdateLegalMoves(Figure.FigureColor color)
    {
        legalMoves.Clear();
        // Получаем список возможных ходов
        List<FigureMove> ownMoves = GetMovesByColor(color, special: true);
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
