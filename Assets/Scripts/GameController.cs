using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public Transform pieces;
    public GameObject legalMoveCell;
    public GameObject illegalMoveCell;

    [HideInInspector]
    public BoardState boardState;

    public void LoadGameObject(Figure figure)
    {
        string figureColor = figure.color.ToString();
        string figureType = figure.GetType().ToString().ToLower();
        string figureName = $"{figureColor}_{figureType}";

        GameObject gameObject = Instantiate(Resources.Load(figureName)) as GameObject;
        gameObject.transform.position = new Vector3(figure.x, figure.y);
        gameObject.transform.parent = GameObject.Find("pieces").transform;

        PieceController pieceController = gameObject.GetComponent<PieceController>();
        pieceController.boardStateFigure = figure;
        figure.movedCallback = pieceController.MovedCallback;
        figure.deletedCallback = pieceController.DeletedCallback;
    }

    void Start()
    {
        boardState = new BoardState();
        List<Figure> figures = boardState.GetFigures();
        foreach(Figure figure in figures)
        {
            LoadGameObject(figure);
        }

    }
}
