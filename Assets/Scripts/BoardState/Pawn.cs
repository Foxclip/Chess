using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// Класс пешки.
/// </summary>
[DataContract(Name = "Pawn")]
public class Pawn : Figure
{
    /// <summary>
    /// Конструктор пешки.
    /// </summary>
    /// <param name="boardState">Доска, на которой будет находиться фигура.</param>
    public Pawn(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) { }

    /// <summary>
    /// Получает все ходы пешки (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае пешки не используется.</param>
    /// <returns>Все ходы пешки (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        List<FigureMove> tempMoveList = new List<FigureMove>();
        int direction = color == FigureColor.white ? 1 : -1;

        // Ход на 1 клетку вперед
        TestCell(tempMoveList, x, y + direction, takePieces: false);
        // Ход на 2 клетки вперед
        if(moveCount == 0 && boardState.GetFigureAtCell(x, y + direction) == null)
        {
            TestCell(tempMoveList, x, y + direction * 2, takePieces: false);
        }
        // Ходы по диагонали
        TestCell(tempMoveList, x - 1, y + direction, freeMove: false);
        TestCell(tempMoveList, x + 1, y + direction, freeMove: false);

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

        // Превращение в ферзя
        if(move.to.y == 7)
        {
            // Удаляем пешку
            Delete();
            // Создаем ферзя
            Queen queen = new Queen(move.to.x, move.to.y, color, boardState);
            boardState.figures.Add(queen);
            boardState.figureCreatedCallback?.Invoke(queen);
        }
    }
}
