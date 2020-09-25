using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Нажатие на доску.
/// </summary>
public class BoardClick : MonoBehaviour
{
    public void ObjectClicked()
    {
        // Убирвем выделение
        PieceController.ClearSelection();
    }
}
