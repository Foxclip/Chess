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
    /// К какой фигуре относится метка.
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
        Figure.FigureColor enemyColor = pieceController.boardStateFigure.GetEnemyColor();

        // Перемещение фигуры
        pieceController.boardStateFigure.ExecuteMove(move);
        gameController.boardState.UpdateLegalMoves();
        Debug.Log($"{enemyColor} has {gameController.boardState.legalMoves.Count()} moves");
        PieceController.ClearSelection();

        // Шах и мат
        if(gameController.boardState.DetectCheck(enemyColor))
        {
            Debug.Log($"CHECK TO {enemyColor} KING");
        }
        if(gameController.boardState.DetectMate())
        {
            Debug.Log($"MATE TO {enemyColor} KING");
        }
    }

}
