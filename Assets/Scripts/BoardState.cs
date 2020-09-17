using System;
using System.Collections;
using System.Collections.Generic;
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
        return base.ToString();
    }

    public static bool operator ==(Vector2Int one, Vector2Int another)
    {
        return (one.x == another.x) && (one.y == another.y);
    }

    public static bool operator!= (Vector2Int one, Vector2Int another)
    {
        return !(one == another);
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
    public Vector2Int Pos { get => new Vector2Int(x, y); }
    public VoidDelegate deletedCallback;
    public IntIntVoidDelegate movedCallback;
    public FigureColor color;
    public BoardState boardState;
    public int moveCount = 0;

    protected List<Vector2Int> tempLegalMoveCells = new List<Vector2Int>();


    public abstract List<Vector2Int> GetAllMoveCells();

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

    public List<Vector2Int> GetLegalMoveCells()
    {
        List<Vector2Int> allMoveCells = GetAllMoveCells();
        List<FigureMove> moves = boardState.legalMoves.FindAll((move) => move.from == Pos);
        List<Vector2Int> moveCells = (from move in moves select move.to).ToList();
        return moveCells;
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
        movedCallback?.Invoke(x, y);
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
        boardState.turnColor = InvertColor(boardState.turnColor);
    }

    public void Move(Vector2Int cell)
    {
        Move(cell.x, cell.y);
    }

    public void Delete()
    {
        deletedCallback?.Invoke();
        boardState.SetFigureAtCell(x, y, null);
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
}

public class Pawn: Figure
{
    public Pawn(int x, int y, FigureColor color, BoardState boardState): base(x, y, color, boardState)
    {
    }

    public override List<Vector2Int> GetAllMoveCells()
    {
        tempLegalMoveCells.Clear();
        int direction = color == FigureColor.white ? 1 : -1;

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
    public Rook(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    public override List<Vector2Int> GetAllMoveCells()
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
    public Knight(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    public override List<Vector2Int> GetAllMoveCells()
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
    public Bishop(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    public override List<Vector2Int> GetAllMoveCells()
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
    public King(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    public override List<Vector2Int> GetAllMoveCells()
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

        //bool kingNotMoved = moveCount == 0;
        ////bool kingsideIsFree = boardState.GetFigureAtCell()
        //tempLegalMoveCells.Add(new Vector2Int(x, y));
        //tempLegalMoveCells.Add(new Vector2Int(x, y));

        return tempLegalMoveCells;
    }

}

public class Queen : Figure
{
    public Queen(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) {}

    public override List<Vector2Int> GetAllMoveCells()
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
    public Figure.FigureColor turnColor;
    public readonly List<FigureMove> legalMoves = new List<FigureMove>();

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

    public List<Figure> GetFigures()
    {
        return (from Figure figure in board where figure != null select figure).ToList();
    }

    public bool DetectCheck(Figure.FigureColor color)
    {
        Figure.FigureColor enemyColor = Figure.InvertColor(color);
        // Получаем фигуры
        List<Figure> figures = GetFigures();
        List<Figure> ownFigures = (from Figure figure in figures where figure.color == color select figure).ToList();
        List<Figure> enemyFigures = (from Figure figure in figures where figure.color == enemyColor select figure).ToList();
        King king = (King)(from Figure figure in ownFigures where figure.GetType() == typeof(King) select figure).First();
        // Ходы короля
        List<Vector2Int> kingMoves = king.GetAllMoveCells();
        // Ходы вражеских фигур
        List<Vector2Int> enemyMoves = new List<Vector2Int>();
        foreach(Figure figure in enemyFigures)
        {
            List<Vector2Int> moves = figure.GetAllMoveCells();
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

    public void UpdateLegalMoves(Figure.FigureColor color)
    {
        legalMoves.Clear();
        // Получаем фигуры
        List<Figure> figures = GetFigures();
        List<Figure> ownFigures = (from Figure figure in figures where figure.color == color select figure).ToList();
        // Получаем свои ходы
        List<FigureMove> ownMoves = new List<FigureMove>();
        foreach(Figure figure in ownFigures)
        {
            List<Vector2Int> moves = figure.GetAllMoveCells();
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
