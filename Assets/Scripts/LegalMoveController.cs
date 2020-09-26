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

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void ObjectClicked()
    {
        gameController.BeginMove(move);
    }

}
