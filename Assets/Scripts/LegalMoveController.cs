using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

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
    private PieceController pieceController;
    private MoveAnimation moveAnimation;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        pieceController = piece.GetComponent<PieceController>();
        moveAnimation = piece.GetComponent<MoveAnimation>();
    }

    /// <summary>
    /// Вызывется после завершения анимации перемещения
    /// </summary>
    public void AfterAnimation()
    {
        pieceController.boardStateFigure.ExecuteMove(move);
        gameController.Turn();
    }

    public void ObjectClicked()
    {
        // Убираем выделение
        PieceController.ClearSelection();
        // Запуск анимации
        moveAnimation.StartAnimation(piece.position, transform.position, AfterAnimation);
    }

}
