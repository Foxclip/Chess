using System.Collections.Generic;
using System.Runtime.Serialization;

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
    public Bishop(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) { }

    /// <summary>
    /// Получает все ходы слона (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае слона не используется.</param>
    /// <returns>Все ходы слона (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        List<FigureMove> tempMoveList = new List<FigureMove>();
        TestDirection(tempMoveList, 1, 1);
        TestDirection(tempMoveList, 1, -1);
        TestDirection(tempMoveList, -1, -1);
        TestDirection(tempMoveList, -1, 1);
        return tempMoveList;
    }

}