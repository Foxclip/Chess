using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    [HideInInspector]
    public int moveCount = 0;
    [HideInInspector]
    public List<Vector2> kingLegalMoves = null;
    [HideInInspector]
    public Figure boardStateFigure;

    private static GameController gameController;
    private static readonly List<GameObject> legalMoveCells = new List<GameObject>();
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
        if(legalMoveCells != null)
        {
            foreach(GameObject obj in legalMoveCells)
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

        // Ставим метки на клетки в которые можно ходить
        List<(int, int)> moveCells = boardStateFigure.GetMoveCells();
        foreach((int, int) cellCoords in moveCells)
        {
            GameObject newObject = Instantiate(gameController.legalMoveCell, new Vector3(cellCoords.Item1, cellCoords.Item2), Quaternion.identity);
            newObject.GetComponent<LegalMoveController>().piece = transform;
            legalMoveCells.Add(newObject);
        }

    }
}
