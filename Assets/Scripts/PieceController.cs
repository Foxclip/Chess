using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

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
    public void MovedCallback(Vector2Int newPos)
    {
        Debug.Log($"Moved callback: {boardStateFigure.GetType()} from {new Vector2Int((int)transform.position.x, (int)transform.position.y)} to {newPos}");
        transform.position = new Vector3(newPos.x, newPos.y);
    }

    /// <summary>
    /// Вызывется фигурой, находящейся на доске BoardState если фигура была удалена (например, ее взяли).
    /// </summary>
    public void DeletedCallback()
    {
        Destroy(gameObject);
    }

    public void ObjectClicked()
    {

        Debug.Log("PIECE CONTROLLER");

        // Нельзя ходить после конца партии
        if(gameController.gameEnded)
        {
            return;
        }

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
        List<FigureMove> allMoves = boardStateFigure.GetAllMoves(special: true);
        List<FigureMove> legalMoves = boardStateFigure.GetLegalMoves();
        List<FigureMove> illegalMoves = allMoves.Except<FigureMove>(legalMoves).ToList();
        // Ставим метки на клетки в которые можно ходить
        foreach(FigureMove move in legalMoves)
        {
            // Создаем метку
            GameObject newObject = Instantiate(gameController.legalMoveCell, new Vector3(move.to.x, move.to.y, -1.0f), Quaternion.identity);
            // Привязываем объект и ход на BoardState
            LegalMoveController legalMoveController = newObject.GetComponent<LegalMoveController>();
            legalMoveController.piece = transform;
            legalMoveController.move = move;
            // Запускаем анимацию появления
            ScaleAnimation scaleAnimation = newObject.GetComponent<ScaleAnimation>();
            scaleAnimation.StartAnimation(0.0f, 1.0f);

            moveCells.Add(newObject);
        }
        // И в которые нельзя из-за шаха
        foreach(FigureMove move in illegalMoves)
        {
            // Создаем метку
            GameObject newObject = Instantiate(gameController.illegalMoveCell, new Vector3(move.to.x, move.to.y, -1.0f), Quaternion.identity);
            // Запускаем анимацию появления
            ScaleAnimation scaleAnimation = newObject.GetComponent<ScaleAnimation>();
            scaleAnimation.StartAnimation(0.0f, 1.0f);

            moveCells.Add(newObject);
        }

    }
}
