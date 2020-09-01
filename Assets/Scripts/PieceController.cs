using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{

    private static GameController gameController;
    private static readonly List<GameObject> legalMoveCells = new List<GameObject>();
    private static GameObject selectedPiece = null;

    public int moveCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    public static Transform GetFigureAtCell(float x, float y)
    {
        foreach(Transform figure in gameController.pieces)
        {
            if(figure.position.x == x && figure.position.y == y)
            {
                return figure;
            }
        }
        return null;
    }

    private bool PlaceLegalMoveCell(float x, float y, string takePieces = "", bool freeMove = true, bool createCell = true, List<Vector2> addToList = null)
    {
        Transform figureAtCell = GetFigureAtCell(x, y);
        bool inBounds = x >= 0 && x < 8 && y >= 0 && y < 8;
        bool canFreeMove = freeMove && figureAtCell == null;
        bool canTakePiece = !takePieces.Equals("") && figureAtCell != null && figureAtCell.name.StartsWith(takePieces);
        if((canFreeMove || canTakePiece) && inBounds)
        {
            if(addToList != null)
            {
                addToList.Add(new Vector2(x, y));
            }
            return true;
        }
        return false;
    }

    private void PlaceLegalMoveCellLine(float x, float y, int p_xOffset, int p_yOffset, bool createCell = true, List<Vector2> addToList = null)
    {
        for(float xOffset = p_xOffset, yOffset = p_yOffset;
            PlaceLegalMoveCell(x + xOffset, y + yOffset, takePieces: GetOppositeName(), createCell: createCell, addToList: addToList);
            xOffset += p_xOffset, yOffset += p_yOffset
        )
        {
            if(GetFigureAtCell(x + xOffset, y + yOffset) != null)
            {
                break;
            }
        }
    }

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

    private string GetOppositeName()
    {
        return transform.name.StartsWith("white") ? "black" : "white";
    }

    private List<Vector2> GetMoveCells()
    {

        List<Vector2> result = new List<Vector2>();

        // Координаты
        float x = transform.position.x;
        float y = transform.position.y;

        // Белая пешка
        if(gameObject.CompareTag("pawn") && gameObject.name.StartsWith("white"))
        {
            PlaceLegalMoveCell(x, y + 1, addToList: result);
            if(moveCount == 0 && GetFigureAtCell(x, y + 1) == null)
            {
                PlaceLegalMoveCell(x, y + 2, addToList: result);
            }
            PlaceLegalMoveCell(x - 1, y - 1, "black", false, addToList: result);
            PlaceLegalMoveCell(x - 1, y + 1, "black", false, addToList: result);
            PlaceLegalMoveCell(x + 1, y - 1, "black", false, addToList: result);
            PlaceLegalMoveCell(x + 1, y + 1, "black", false, addToList: result);
        }

        // Черная пешка
        if(gameObject.CompareTag("pawn") && gameObject.name.StartsWith("black"))
        {
            PlaceLegalMoveCell(x, y - 1, addToList: result);
            if(moveCount == 0 && GetFigureAtCell(x, y - 1) == null)
            {
                PlaceLegalMoveCell(x, y - 2, addToList: result);
            }
            PlaceLegalMoveCell(x - 1, y - 1, "white", false, addToList: result);
            PlaceLegalMoveCell(x - 1, y + 1, "white", false, addToList: result);
            PlaceLegalMoveCell(x + 1, y - 1, "white", false, addToList: result);
            PlaceLegalMoveCell(x + 1, y + 1, "white", false, addToList: result);
        }

        // Ладья
        if(gameObject.CompareTag("rook"))
        {
            PlaceLegalMoveCellLine(x, y, 0, 1, addToList: result);
            PlaceLegalMoveCellLine(x, y, 1, 0, addToList: result);
            PlaceLegalMoveCellLine(x, y, 0, -1, addToList: result);
            PlaceLegalMoveCellLine(x, y, -1, 0, addToList: result);
        }

        // Конь
        if(gameObject.CompareTag("knight"))
        {
            PlaceLegalMoveCell(x - 1, y + 2, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 1, y + 2, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 2, y - 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 2, y + 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x - 1, y - 2, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 1, y - 2, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x - 2, y + 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x - 2, y - 1, GetOppositeName(), addToList: result);
        }

        // Слон
        if(gameObject.CompareTag("bishop"))
        {
            PlaceLegalMoveCellLine(x, y, 1, 1, addToList: result);
            PlaceLegalMoveCellLine(x, y, 1, -1, addToList: result);
            PlaceLegalMoveCellLine(x, y, -1, -1, addToList: result);
            PlaceLegalMoveCellLine(x, y, -1, 1, addToList: result);
        }

        // Король
        if(gameObject.CompareTag("king"))
        {
            PlaceLegalMoveCell(x - 1, y - 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x - 1, y + 0, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x - 1, y + 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 0, y + 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 1, y + 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 1, y + 0, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 1, y - 1, GetOppositeName(), addToList: result);
            PlaceLegalMoveCell(x + 0, y - 1, GetOppositeName(), addToList: result);
        }

        // Ферзь
        if(gameObject.CompareTag("queen"))
        {
            PlaceLegalMoveCellLine(x, y, 0, 1, addToList: result);
            PlaceLegalMoveCellLine(x, y, 1, 0, addToList: result);
            PlaceLegalMoveCellLine(x, y, 0, -1, addToList: result);
            PlaceLegalMoveCellLine(x, y, -1, 0, addToList: result);
            PlaceLegalMoveCellLine(x, y, 1, 1, addToList: result);
            PlaceLegalMoveCellLine(x, y, 1, -1, addToList: result);
            PlaceLegalMoveCellLine(x, y, -1, -1, addToList: result);
            PlaceLegalMoveCellLine(x, y, -1, 1, addToList: result);
        }

        return result;
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
        List<Vector2> moveCells = GetMoveCells();
        foreach(Vector2 cellCoords in moveCells)
        {
            GameObject newObject = Instantiate(gameController.legalMoveCell, new Vector3(cellCoords.x, cellCoords.y), Quaternion.identity);
            newObject.GetComponent<LegalMoveController>().piece = transform;
            legalMoveCells.Add(newObject);
        }

    }
}
