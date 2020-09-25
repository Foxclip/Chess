using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// Метка арзрешенного хода.
/// </summary>
public class LegalMoveController : MonoBehaviour, IPointerDownHandler
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
    private ScaleAnimation scaleAnimation;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        scaleAnimation = GetComponent<ScaleAnimation>();
        scaleAnimation.StartAnimation(0.0f, 1.0f);
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        PieceController pieceController = piece.GetComponent<PieceController>();

        // Перемещение фигуры
        pieceController.boardStateFigure.ExecuteMove(move);
        gameController.Turn();
    }

}
