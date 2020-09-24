using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
    /// Текст на экране после завершения партии.
    /// </summary>
    public Image endgameText;

    /// <summary>
    /// Доска BoardState, на которой находятся фигуры.
    /// </summary>
    [HideInInspector]
    public BoardState boardState;

    /// <summary>
    /// Состояние после завершения партии.
    /// </summary>
    public bool gameEnded = false;

    /// <summary>
    /// Если этот файл, существует, то состояние доски будет загружено из него.
    /// Сохранение идет также в этот файл.
    /// </summary>
    public const string savedStateFileName = "boardState.xml";

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
        // Если состояние доски было сохранено в файл, загружаем из файла
        if(File.Exists(savedStateFileName))
        {
            boardState = BoardState.Deserialize(savedStateFileName);
        }
        else
        {
            boardState = new BoardState();
        }
        // Создаем объекты Unity и привязываем их к BoardState фигурам.
        List<Figure> figures = boardState.GetFigures();
        foreach(Figure figure in figures)
        {
            LoadGameObject(figure);
        }
    }

    private void Update()
    {
        // Настройка камеры
        Camera camera = Camera.main;
        if(camera.aspect >= 1)
        {
            camera.orthographicSize = 4.0f;
        }
        else
        {
            camera.orthographicSize = 4.0f / camera.aspect;
        }

        // Клавиатура
        if(Input.GetKeyDown("s"))
        {
            // Сохраняем состояние доски в файл
            boardState.Serialize(savedStateFileName);
            Debug.Log($"Saved to file {savedStateFileName}");
        }
    }

    public void Turn()
    {
        // Убираем выделение
        PieceController.ClearSelection();

        // Количество разрешенных ходов
        Debug.Log($"{boardState.turnColor} has {boardState.GetLegalMoves().Count} moves");

        // Шах и мат
        if(boardState.DetectCheck(boardState.turnColor))
        {
            Debug.Log($"CHECK TO {boardState.turnColor} KING");
        }
        if(boardState.DetectMate())
        {
            Debug.Log($"MATE TO {boardState.turnColor} KING");
            gameEnded = true;
            endgameText.gameObject.SetActive(true);
            string text = boardState.turnColor == Figure.FigureColor.black ? "Белые выиграли" : "Черные выграли";
            endgameText.transform.GetChild(0).GetComponent<Text>().text = text;
            return;
        }

        // Ход ИИ
        if(boardState.turnColor == Figure.FigureColor.black)
        {
            AiModule.AiMove(boardState);
            Turn();
        }
    }
}
