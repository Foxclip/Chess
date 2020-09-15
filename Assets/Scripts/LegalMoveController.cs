using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LegalMoveController : MonoBehaviour
{

    public Transform piece;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        // Перемещение фигуры
        PieceController pieceController = piece.GetComponent<PieceController>();
        pieceController.boardStateFigure.Move((int)transform.position.x, (int)transform.position.y);
        PieceController.ClearSelection();

        // Шах
        gameController.board.DetectMate(pieceController.boardStateFigure.GetEnemyColor());

    }

}
