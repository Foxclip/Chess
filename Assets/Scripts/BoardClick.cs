using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Нажатие на доску.
/// </summary>
public class BoardClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Убирвем выделение
        PieceController.ClearSelection();
    }
}
