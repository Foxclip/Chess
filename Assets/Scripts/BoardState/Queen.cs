using System.Collections.Generic;
using System.Runtime.Serialization;

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
    public Queen(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) { }

    /// <summary>
    /// Получает все ходы ферзя (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае ферзя не используется.</param>
    /// <returns>Все ходы ферзя (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        List<FigureMove> tempMoveList = new List<FigureMove>();
        TestDirection(tempMoveList, 0, 1);
        TestDirection(tempMoveList, 1, 0);
        TestDirection(tempMoveList, 0, -1);
        TestDirection(tempMoveList, -1, 0);
        TestDirection(tempMoveList, 1, 1);
        TestDirection(tempMoveList, 1, -1);
        TestDirection(tempMoveList, -1, -1);
        TestDirection(tempMoveList, -1, 1);
        return tempMoveList;
    }
}