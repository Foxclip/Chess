﻿using System;
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
    /// Ход фигуры во время анимации перемещения
    /// </summary>
    private FigureMove currentMove;

    /// <summary>
    /// GameObject фигуры, который перемещается.
    /// </summary>
    private GameObject currentMovingPiece;

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
        GameObject gameObject = Instantiate(Resources.Load($"Pieces/{figureName}")) as GameObject;
        gameObject.transform.position = new Vector3(figure.x, figure.y);
        gameObject.transform.parent = GameObject.Find("pieces").transform;

        // Привязываем объект к фигуре BoardState
        PieceController pieceController = gameObject.GetComponent<PieceController>();
        pieceController.boardStateFigure = figure;
        figure.deletedCallback = pieceController.DeletedCallback;
    }

    void Start()
    {
        // Если состояние доски было сохранено в файл, загружаем из файла
        if(File.Exists(savedStateFileName))
        {
            boardState = BoardState.Deserialize(savedStateFileName, FigureCreatedCallback);
        }
        else
        {
            boardState = new BoardState(FigureCreatedCallback);
        }
        // Создаем объекты Unity и привязываем их к BoardState фигурам.
        List<Figure> figures = boardState.GetFigures();
        foreach(Figure figure in figures)
        {
            LoadGameObject(figure);
        }
    }

    void Update()
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
        // Мышь (или тап на экран)
        if(Input.GetMouseButtonDown(0))
        {
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                hit.collider.gameObject.BroadcastMessage("ObjectClicked", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    /// <summary>
    /// Вызывется при создании фигуры
    /// </summary>
    /// <param name="figure"></param>
    private void FigureCreatedCallback(Figure figure)
    {
        LoadGameObject(figure);
    }

    /// <summary>
    /// Находит GameObject фигуры по ее позиции на доске
    /// </summary>
    /// <param name="pos">Позиция фигуры на BoardState</param>
    private GameObject FindGameObjectByPos(Vector2Int pos)
    {
        Transform pieces = GameObject.Find("pieces").transform;
        List<Transform> foundPieces = new List<Transform>();
        foreach(Transform piece in pieces)
        {
            if(piece.position.x == pos.x && piece.position.y == pos.y)
            {
                foundPieces.Add(piece);
            }
        }
        if(foundPieces.Count == 0)
        {
            throw new InvalidOperationException($"Не найден GameObject фигуры в позиции {pos}");
        }
        if(foundPieces.Count > 1)
        {
            throw new InvalidOperationException($"Найдено {foundPieces.Count} фигуры в позиции {pos}");
        }
        return foundPieces[0].gameObject;
    }

    /// <summary>
    /// Запуск анимации перемещения фигуры. Ход фигуры на BoardState выполняется после завершения анимации.
    /// </summary>
    /// <param name="move"></param>
    public void BeginMove(FigureMove move)
    {
        // Убираем выделение
        PieceController.ClearSelection();

        // Находим GameObject фигуры
        GameObject piece = FindGameObjectByPos(move.from);

        // Перемещаем спрайт на слой выше
        SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "CurrentPiece";
        currentMovingPiece = piece;

        // Запускаем анимацию
        MoveAnimation moveAnimation = piece.GetComponent<MoveAnimation>();
        currentMove = move;
        moveAnimation.StartAnimation(new Vector3(move.from.x, move.from.y), new Vector3(move.to.x, move.to.y), EndMove);
    }

    /// <summary>
    /// Вызывется после завершения анимации перемещения фигуры.
    /// </summary>
    public void EndMove()
    {
        // Делаем ход на BoardState
        boardState.ExecuteMove(currentMove);

        // Перемещаем спрайт на слой ниже
        SpriteRenderer spriteRenderer = currentMovingPiece.GetComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Pieces";
        currentMovingPiece = null;

        // Обновляем список разрешенных ходов
        boardState.UpdateLegalMoves();
        Debug.Log($"{boardState.turnColor} has {boardState.GetLegalMoves().Count} moves");

        // Шах
        if(boardState.DetectCheck(boardState.turnColor))
        {
            Debug.Log($"CHECK TO {boardState.turnColor} KING");
        }
        // Мат
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
            FigureMove aiMove = AiModule.AiMove(boardState);
            BeginMove(aiMove);
        }
    }
}
