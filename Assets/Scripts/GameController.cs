using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    /// <summary>
    /// Метка разрешенного хода.
    /// </summary>
    public GameObject legalMoveCell;
    /// <summary>
    /// Метка запрещенного хода.
    /// </summary>
    public GameObject illegalMoveCell;

    /// <summary>
    /// Доска BoardState, на которой находятся фигуры.
    /// </summary>
    [HideInInspector]
    public BoardState boardState;

    /// <summary>
    /// Загружает префаб фигуры и привязывает его к фигуре BoardState.
    /// </summary>
    /// <param name="figure">Фигура BoardState.</param>
    public void LoadGameObject(Figure figure)
    {
        // Определяем имя префаба
        string figureColor = figure.color.ToString();
        string figureType = figure.GetType().ToString().ToLower();
        string figureName = $"{figureColor}_{figureType}";

        // Создаем объект
        GameObject gameObject = Instantiate(Resources.Load(figureName)) as GameObject;
        gameObject.transform.position = new Vector3(figure.x, figure.y);
        gameObject.transform.parent = GameObject.Find("pieces").transform;

        // Привязываем объект к фигуре BoardState
        PieceController pieceController = gameObject.GetComponent<PieceController>();
        pieceController.boardStateFigure = figure;
        figure.movedCallback = pieceController.MovedCallback;
        figure.deletedCallback = pieceController.DeletedCallback;
    }

    void Start()
    {
        // Создаем доску BoardState и расставляем фигуры
        boardState = new BoardState();
        // Создаем объекты привязываем их к BoardState фигурам.
        List<Figure> figures = boardState.GetFigures();
        foreach(Figure figure in figures)
        {
            LoadGameObject(figure);
        }
    }

    private void Update()
    {
        // Сохраняем состояние доски в файл
        if(Input.GetKeyDown("s"))
        {
            string filename = "boardState.xml";
            boardState.Serialize(filename);
            Debug.Log($"Saved to file {filename}");
        }
    }

    public void Turn()
    {
        // Убираем выделение
        PieceController.ClearSelection();

        Debug.Log($"{boardState.turnColor} has {boardState.GetLegalMoves().Count} moves");

        // Шах и мат
        if(boardState.DetectCheck(boardState.turnColor))
        {
            Debug.Log($"CHECK TO {boardState.turnColor} KING");
        }
        if(boardState.DetectMate())
        {
            Debug.Log($"MATE TO {boardState.turnColor} KING");
        }

        // Ход ИИ
        if(boardState.turnColor == Figure.FigureColor.black)
        {
            AiModule.AiMove(boardState);
            Turn();
        }
    }
}
