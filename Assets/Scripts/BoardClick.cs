using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Нажатие на доску.
/// </summary>
public class BoardClick : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        // Убирвем выделение
        PieceController.ClearSelection();
    }
}
