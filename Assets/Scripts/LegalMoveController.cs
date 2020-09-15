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
        PieceController pieceController = piece.GetComponent<PieceController>();
        string enemyColor = pieceController.boardStateFigure.GetEnemyColor();

        // Перемещение фигуры
        pieceController.boardStateFigure.Move((int)transform.position.x, (int)transform.position.y);
        gameController.board.UpdateLegalMoves(enemyColor);
        Debug.Log($"{enemyColor} has {gameController.board.legalMoves.Count()} moves");
        PieceController.ClearSelection();

        // Шах
        if(gameController.board.DetectCheck(enemyColor))
        {
            Debug.Log($"CHECK TO {enemyColor} KING");
        }
        if(gameController.board.DetectMate())
        {
            Debug.Log($"MATE TO {enemyColor} KING");
        }
    }

}
