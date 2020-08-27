using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{

    private static GameController gameController;
    private static GameObject selectedPiece = null;
    private static List<GameObject> legalMoveCells = new List<GameObject>();

    public int moveCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

    private void PlaceLegalMoveCell(float x, float y, string takePieces = "", bool freeMove = true)
    {
        Transform figureAtCell = GetFigureAtCell(x, y);
        bool inBounds = x >= 0 && x < 8 && y >= 0 && y < 8;
        bool canFreeMove = freeMove && figureAtCell == null;
        bool canTakePiece = !takePieces.Equals("") && figureAtCell != null && figureAtCell.name.StartsWith(takePieces);
        //Debug.Log($"Checking {x}:{y}: inBounds:{inBounds} canFreeMove:{canFreeMove} canTakePiece:{canTakePiece}");
        if((canFreeMove || canTakePiece) && inBounds)
        {
            GameObject newObject = Instantiate(gameController.legalMoveCell, new Vector3(x, y), Quaternion.identity);
            newObject.GetComponent<LegalMoveController>().piece = transform;
            legalMoveCells.Add(newObject);
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

    private void OnMouseDown()
    {
        Debug.Log($"PRESSED {gameObject.name}");
        ClearSelection();
        selectedPiece = gameObject;
        float x = transform.position.x;
        float y = transform.position.y;
        if(gameObject.tag == "pawn" && gameObject.name.StartsWith("white"))
        {
            PlaceLegalMoveCell(x, y + 1);
            if(moveCount == 0)
            {
                PlaceLegalMoveCell(x, y + 2);
            }
            PlaceLegalMoveCell(x - 1, y - 1, "black", false);
            PlaceLegalMoveCell(x - 1, y + 1, "black", false);
            PlaceLegalMoveCell(x + 1, y - 1, "black", false);
            PlaceLegalMoveCell(x + 1, y + 1, "black", false);
        }
    }
}
