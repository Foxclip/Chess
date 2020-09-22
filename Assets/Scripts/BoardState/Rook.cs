using System.Collections.Generic;
using System.Runtime.Serialization;

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
    public Rook(int x, int y, FigureColor color, BoardState boardState) : base(x, y, color, boardState) { }

    /// <summary>
    /// Получает все ходы ладьи (включая ходы приводящие к шаху).
    /// </summary>
    /// <param name="special">В случае ладьи не используется.</param>
    /// <returns>Все ходы ладьи (включая ходы приводящие к шаху).</returns>
    public override List<FigureMove> GetAllMoves(bool special)
    {
        List<FigureMove> tempMoveList = new List<FigureMove>();
        TestDirection(tempMoveList, 0, 1);
        TestDirection(tempMoveList, 1, 0);
        TestDirection(tempMoveList, 0, -1);
        TestDirection(tempMoveList, -1, 0);
        return tempMoveList;
    }
}