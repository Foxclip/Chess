using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Нажатие на доску.
/// </summary>
public class BoardClick : MonoBehaviour
{
    GameController gameController;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void ObjectClicked()
    {
        // Если интерфейс заблокирован, то выбирать ходить нельзя
        if(gameController.interfaceLocked)
        {
            return;
        }
        // Убираем выделение
        PieceController.ClearSelection();
    }
}
