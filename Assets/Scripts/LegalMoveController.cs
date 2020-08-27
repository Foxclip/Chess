using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Debug.Log("LEGAL MOVE");
        Transform figureAtCell = PieceController.GetFigureAtCell(transform.position.x, transform.position.y);
        if(figureAtCell != null)
        {
            Destroy(figureAtCell.gameObject);
        }
        piece.position = transform.position;
        piece.GetComponent<PieceController>().moveCount++;
        PieceController.ClearSelection();
    }

}
