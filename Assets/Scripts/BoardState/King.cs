using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
    public King(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) { }

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