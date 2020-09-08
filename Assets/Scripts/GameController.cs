using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public Transform pieces;
    public GameObject legalMoveCell;

    private BoardState board;

    // Start is called before the first frame update
    void Start()
    {
        board = new BoardState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
