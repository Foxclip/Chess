using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Метка арзрешенного хода.
/// </summary>
public class LegalMoveController : MonoBehaviour
{

    /// <summary>
    /// К какому Unity объекту относится метка.
    /// </summary>
    public Transform piece;
    /// <summary>
    /// Привязанный ход на BoardState
    /// </summary>
    [HideInInspector]
    public FigureMove move;

    private GameController gameController;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    private void OnMouseDown()
    {
        PieceController pieceController = piece.GetComponent<PieceController>();

        // Перемещение фигуры
        pieceController.boardStateFigure.ExecuteMove(move);
        gameController.Turn();
    }

}
