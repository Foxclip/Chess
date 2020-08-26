using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{

    private static GameController gameController;
    private static GameObject selectedPiece = null;
    private static List<GameObject> legalMoveCells = new List<GameObject>();

    private int moveCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private static Transform GetFigureAtCell(float x, float y)
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

    private static void PlaceLegalMoveCell(float x, float y)
    {
        if(GetFigureAtCell(x, y) == null)
        {
            GameObject newObject = Instantiate(gameController.legalMoveCell, new Vector3(x, y), Quaternion.identity);
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
        if(gameObject.tag == "pawn" && gameObject.name.StartsWith("white"))
        {
            PlaceLegalMoveCell(transform.position.x, transform.position.y + 1);
            if(moveCount == 0)
            {
                PlaceLegalMoveCell(transform.position.x, transform.position.y + 2);
            }
        }
    }
}
