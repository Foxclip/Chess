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

    private GameController gameController;
    private ImplodeAnimation implodeAnimation;
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
        implodeAnimation = GetComponent<ImplodeAnimation>();
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
    /// Вызывется фигурой, находящейся на доске BoardState если фигура была удалена (например, ее взяли).
    /// </summary>
    public void DeletedCallback()
    {
        // Перемещаем спрайт на слой ниже
        SpriteRenderer spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "ImplodingPieces";
        // Запускаем анимацию удаления
        implodeAnimation.StartAnimation(1.0f, 0.0f);
    }

    public void ObjectClicked()
    {
        // Если интерфейс заблокирован, то выбирать фигуры нельзя
        if(gameController.interfaceLocked)
        {
            return;
        }

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
        List<FigureMove> allMArkedMoves = boardStateFigure.GetAllMarkedMoves();
        List<FigureMove> legalMoves = allMArkedMoves.FindAll(move => move.attackingFigures.Count == 0);
        List<FigureMove> illegalMoves = allMArkedMoves.Except(legalMoves).ToList();
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
            // Привязываем объект и ход на BoardState
            IllegalMoveController illegalMoveController = newObject.GetComponent<IllegalMoveController>();
            illegalMoveController.piece = transform;
            illegalMoveController.move = move;
            // Запускаем анимацию появления
            ScaleAnimation scaleAnimation = newObject.GetComponent<ScaleAnimation>();
            scaleAnimation.StartAnimation(0.0f, 1.0f);

            moveCells.Add(newObject);
        }

    }
}
