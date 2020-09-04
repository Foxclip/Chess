using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LegalMoveController : MonoBehaviour
{

    public Transform piece;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        // Перемещение фигуры
        Transform figureAtCell = PieceController.GetFigureAtCell(transform.position.x, transform.position.y);
        if(figureAtCell != null)
        {
            Destroy(figureAtCell.gameObject);
        }
        piece.position = transform.position;
        piece.GetComponent<PieceController>().moveCount++;
        PieceController.ClearSelection();

        // Определение того, был ли поставлен шах вражескому королю

        // Ходы короля
        string namePrefix = piece.GetComponent<PieceController>().GetEnemyNamePrefix();
        GameObject enemyKing = GameObject.Find(namePrefix + "_king");
        PieceController enemyKingPieceController = enemyKing.GetComponent<PieceController>();
        List<Vector2> enemyKingMoves = enemyKingPieceController.GetMoveCells();

        // Ходы своих фигур
        Transform pieces = GameObject.Find("pieces").transform;
        string ownPrefix = piece.GetComponent<PieceController>().GetNamePrefix();
        var ownPieces = from Transform piece
                        in pieces
                        where piece.name.StartsWith(ownPrefix)
                        select piece;
        List<Vector2> ownMoves = new List<Vector2>();
        foreach(Transform piece in ownPieces)
        {
            ownMoves.AddRange(piece.GetComponent<PieceController>().GetMoveCells());
        }

        // Определение шаха
        if(ownMoves.Contains(enemyKing.transform.position))
        {
            Debug.Log($"Check to {enemyKingPieceController.GetNamePrefix()} king");
        }

        // Определение оставшихся ходов короля
        enemyKingMoves.RemoveAll((kingMove) => ownMoves.Contains(kingMove));
        List<Vector2> kingLegalMoves = enemyKingMoves;
        enemyKingPieceController.kingLegalMoves = kingLegalMoves;
    }

}
