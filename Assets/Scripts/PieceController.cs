using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PieceController : MonoBehaviour
{
    [HideInInspector]
    public int moveCount = 0;
    [HideInInspector]
    public List<Vector2> kingLegalMoves = null;
    [HideInInspector]
    public Figure boardStateFigure;

    private static GameController gameController;
    private static readonly List<GameObject> moveCells = new List<GameObject>();
    private static GameObject selectedPiece = null;



    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    //void Update()
    //{
        
    //}

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

        // Ставим выделение на данную фигуру
        selectedPiece = gameObject;

        List<Vector2Int> allMoveCells = boardStateFigure.GetAllMoveCells();
        List<Vector2Int> legalMoveCells = boardStateFigure.GetLegalMoveCells();
        List<Vector2Int> illegalMoveCells = allMoveCells.Except(legalMoveCells).ToList();
        // Ставим метки на клетки в которые можно ходить
        foreach(Vector2Int cellCoords in legalMoveCells)
        {
            GameObject newObject = Instantiate(gameController.legalMoveCell, new Vector3(cellCoords.x, cellCoords.y), Quaternion.identity);
            newObject.GetComponent<LegalMoveController>().piece = transform;
            moveCells.Add(newObject);
        }
        // И в которые нельзя из-за шаха
        foreach(Vector2Int cellCoords in illegalMoveCells)
        {
            GameObject newObject = Instantiate(gameController.illegalMoveCell, new Vector3(cellCoords.x, cellCoords.y), Quaternion.identity);
            moveCells.Add(newObject);
        }

    }
}
