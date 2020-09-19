using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PieceController : MonoBehaviour
{
    /// <summary>
    /// Привязанная к объекту фигура на доке BoardState.
    /// </summary>
    [HideInInspector]
    public Figure boardStateFigure;

    private static GameController gameController;
    /// <summary>
    /// Метки ходов.
    /// </summary>
    private static readonly List<GameObject> moveCells = new List<GameObject>();
    /// <summary>
    /// Выбранная фигура.
    /// </summary>
    private static GameObject selectedPiece = null;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    /// <summary>
    /// Очистиит выделение (удалить все метки ходов).
    /// </summary>
    public static void ClearSelection()
    {
        selectedPiece = null;
        if(moveCells != null)
        {
            foreach(GameObject obj in moveCells)
            {
                Destroy(obj);
            }
        }
    }

    /// <summary>
    /// Вызывется фигурой, находящейся на доске BoardState если фигура была передвинута.
    /// </summary>
    public void MovedCallback(int newX, int newY)
    {
        Debug.Log($"Moved callback: {boardStateFigure.GetType()} from {new Vector2Int((int)transform.position.x, (int)transform.position.y)} to {new Vector2Int(newX, newY)}");
        transform.position = new Vector3(newX, newY);
    }

    /// <summary>
    /// Вызывется фигурой, находящейся на доске BoardState если фигура была удалена (например, ее взяли).
    /// </summary>
    public void DeletedCallback()
    {
        Destroy(gameObject);
    }

    private void OnMouseDown()
    {
        // Если фигура выбрана, убираем выделение
        if(selectedPiece == gameObject)
        {
            ClearSelection();
            return;
        }

        // Убираем выделение с другой фигуры
        ClearSelection();

        // Нельзя ходить вражеской фигурой
        if(gameController.boardState.turnColor != gameObject.GetComponent<PieceController>().boardStateFigure.color)
        {
            return;
        }

        // Ставим выделение на данную фигуру
        selectedPiece = gameObject;

        // Определяем клеки в которые можно ходить и в которые нельзя
        List<Vector2Int> allMoveDestinations = BoardState.GetMoveDestinations(boardStateFigure.GetAllMoves(special: true));
        List<Vector2Int> legalMoveDestinations = BoardState.GetMoveDestinations(boardStateFigure.GetLegalMoves());
        List<Vector2Int> illegalMoveDestinations = allMoveDestinations.Except(legalMoveDestinations).ToList();
        // Ставим метки на клетки в которые можно ходить
        foreach(Vector2Int cellCoords in legalMoveDestinations)
        {
            GameObject newObject = Instantiate(gameController.legalMoveCell, new Vector3(cellCoords.x, cellCoords.y), Quaternion.identity);
            newObject.GetComponent<LegalMoveController>().piece = transform;
            moveCells.Add(newObject);
        }
        // И в которые нельзя из-за шаха
        foreach(Vector2Int cellCoords in illegalMoveDestinations)
        {
            GameObject newObject = Instantiate(gameController.illegalMoveCell, new Vector3(cellCoords.x, cellCoords.y), Quaternion.identity);
            moveCells.Add(newObject);
        }

    }
}
