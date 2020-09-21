using System.Collections.Generic;
using System.Runtime.Serialization;

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
    public Knight(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) { }

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