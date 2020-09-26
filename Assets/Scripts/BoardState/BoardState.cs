using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using UnityEngine;

/// <summary>
/// Класс доски.
/// </summary>
[DataContract(Name = "BoardState", IsReference = true)]
public class BoardState
{
    /// <summary>
    /// Список фигур.
    /// </summary>
    [DataMember]
    public readonly List<Figure> figures = new List<Figure>();
    /// <summary>
    /// Цвет фигур, которые сейчас ходят.
    /// </summary>
    [DataMember]
    public Figure.FigureColor turnColor;

    /// <summary>
    /// Временный список разрешенных ходов цвета, который сейчас ходит.
    /// </summary>
    private List<FigureMove> legalMoves = new List<FigureMove>();

    /// <summary>
    /// Вызывается при создании фигуры.
    /// </summary>
    public Action<Figure> figureCreatedCallback;

    /// <summary>
    /// Конструктор доски. Заполяет доску фигурами как в начале партии.
    /// </summary>
    public BoardState(Action<Figure> figureCreatedCallback = null)
    {
        // Коллбек, вызывающийся при создании фигуры
        this.figureCreatedCallback = figureCreatedCallback;

        // Сокращенные названия
        Figure.FigureColor white = Figure.FigureColor.white;
        Figure.FigureColor black = Figure.FigureColor.black;

        // Белые пешки
        for(int x = 0; x < 8; x++)
        {
            figures.Add(new Pawn(x, 1, white, this));
        }
        // Черные пешки
        for(int x = 0; x < 8; x++)
        {
            figures.Add(new Pawn(x, 6, black, this));
        }
        // Ладьи
        figures.Add(new Rook(0, 0, white, this));
        figures.Add(new Rook(7, 0, white, this));
        figures.Add(new Rook(0, 7, black, this));
        figures.Add(new Rook(7, 7, black, this));
        // Кони
        figures.Add(new Knight(1, 0, white, this));
        figures.Add(new Knight(6, 0, white, this));
        figures.Add(new Knight(1, 7, black, this));
        figures.Add(new Knight(6, 7, black, this));
        // Слоны
        figures.Add(new Bishop(2, 0, white, this));
        figures.Add(new Bishop(5, 0, white, this));
        figures.Add(new Bishop(2, 7, black, this));
        figures.Add(new Bishop(5, 7, black, this));
        // Короли
        figures.Add(new King(4, 0, white, this));
        figures.Add(new King(4, 7, black, this));
        // Ферзи
        figures.Add(new Queen(3, 0, white, this));
        figures.Add(new Queen(3, 7, black, this));

        // Белые ходят первыми
        turnColor = Figure.FigureColor.white;

        // Обновляем список разрешенных ходов
        UpdateLegalMoves();
    }

    /// <summary>
    /// Копирующий конструктор. Фигуры также копируются.
    /// </summary>
    public BoardState(BoardState original)
    {
        turnColor = original.turnColor;
        figures = new List<Figure>();
        foreach(Figure figure in original.figures)
        {
            figures.Add(figure.Copy(this));
        }
    }

    /// <summary>
    /// Определяет, находятся ли координаты в пределах доски.
    /// </summary>
    /// <returns>Находятся ли координаты в пределах доски.</returns>
    public static bool CoordinatesInBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }

    /// <summary>
    /// Определяет, находятся ли координаты в пределах доски.
    /// </summary>
    /// <returns>Находятся ли координаты в пределах доски.</returns>
    public static bool CoordinatesInBounds(Vector2Int coords)
    {
        return CoordinatesInBounds(coords.x, coords.y);
    }

    /// <summary>
    /// Получает список клеток назначения из списка ходов.
    /// </summary>
    /// <param name="moves">Список ходов.</param>
    /// <returns>Список клеток назначения (без повторяющихся эелементов).</returns>
    public static List<Vector2Int> GetMoveDestinations(List<FigureMove> moves)
    {
        return (from move in moves select move.to).Distinct().ToList();
    }

    /// <summary>
    /// Получает фигуру, находящуюся в определенной клетке.
    /// </summary>
    /// <returns>Фигура стоящая в клетке или null если там ничего нет.</returns>
    public Figure GetFigureAtCell(int x, int y)
    {
        return GetFigureAtCell(new Vector2Int(x, y));
    }

    /// <summary>
    /// Получает фигуру, находящуюся в определенной клетке.
    /// </summary>
    /// <returns>Фигура стоящая в клетке или null если там ничего нет.</returns>
    public Figure GetFigureAtCell(Vector2Int cell)
    {
        List<Figure> figuresAtCell = figures.FindAll(figure => figure.Pos == cell);
        if(figuresAtCell.Count > 1)
        {
            string figureStr = "";
            foreach(Figure figure in figuresAtCell)
            {
                figureStr += " " + figure.GetType().Name;
            }

            throw new InvalidOperationException($"В клетке {cell} находятся {figuresAtCell.Count} фигуры: {figureStr}");
        }
        if(figuresAtCell.Count == 0)
        {
            return null;
        }
        return figuresAtCell[0];
    }

    /// <summary>
    /// Получает все фигуры на доске.
    /// </summary>
    /// <returns>Список фигур на доске.</returns>
    public List<Figure> GetFigures()
    {
        return figures;
    }

    /// <summary>
    /// Возвращает список фигур атакующих фигуру.
    /// </summary>
    /// <param name="color">Фигура, которую атакуют.</param>
    public List<Figure> GetFiguresAttackingFigure(Figure figure)
    {
        Figure.FigureColor enemyColor = figure.GetEnemyColor();
        List<FigureMove> colorMoves = GetMovesByColor(enemyColor, special: false);
        List<FigureMove> attackingMoves = colorMoves.FindAll(move => move.to == figure.Pos).ToList();
        List<Figure> attackingFigures = attackingMoves.Select(move => GetFigureAtCell(move.from)).ToList();
        return attackingFigures;
    }

    /// <summary>
    /// Определяет, находится ли любая из клеток из списка под боем фигур определенного цвета.
    /// </summary>
    /// <param name="color">Клетки находятся под боем фигур этого цвета.</param>
    public bool AnyCellIsUnderAttack(List<Vector2Int> cells, Figure.FigureColor color)
    {
        return cells.Intersect(GetAllCellsUnderAttackByColor(color)).Count() > 0;
    }

    /// <summary>
    /// Возвращает короля определенного цвета.
    /// </summary>
    public King FindKingByColor(Figure.FigureColor color)
    {
        List<Figure> ownFigures = GetFiguresByColor(color);
        King king = (King)ownFigures.Find(figure => figure.GetType() == typeof(King));
        return king;
    }

    /// <summary>
    /// Возвращает список фигур атакующих короля определенного цвета.
    /// </summary>
    /// <param name="color">Королю какого цвета поствлен шах.</param>
    public bool DetectCheck(Figure.FigureColor color)
    {
        return GetFiguresAttackingFigure(FindKingByColor(color)).Count > 0;
    }

    /// <summary>
    /// Возвращает список фигур атакующих заданного короля.
    /// </summary>
    /// <param name="color">Королю какого цвета поствлен шах.</param>
    public bool DetectCheck(King king)
    {
        return GetFiguresAttackingFigure(king).Count > 0;
    }

    /// <summary>
    /// Определяет, поставлен ли королю того цвета который сейчас ходит мат.
    /// </summary>
    public bool DetectMate()
    {
        // Определение мата
        return GetLegalMoves().Count == 0;
    }

    /// <summary>
    /// Получает список фигур определенного цвета.
    /// </summary>
    public List<Figure> GetFiguresByColor(Figure.FigureColor color)
    {
        List<Figure> allFigures = GetFigures();
        List<Figure> colorFigures = allFigures.FindAll(figure => figure.color == color);
        return colorFigures;
    }

    /// <summary>
    /// Получает список ходов (в том числе приводящих к шаху) фигур определенного цвета.
    /// </summary>
    public List<FigureMove> GetMovesByColor(Figure.FigureColor color, bool special)
    {
        // Получаем фигуры
        List<Figure> figures = GetFiguresByColor(color);
        // Получаем список ходов
        List<FigureMove> colorMoves = figures.SelectMany(figure => figure.GetAllMoves(special)).ToList();
        return colorMoves;
    }

    /// <summary>
    /// Получает все клетки, находящиеся под боем фигур определенного цвета
    /// </summary>
    public List<Vector2Int> GetAllCellsUnderAttackByColor(Figure.FigureColor color)
    {
        // Получаем фигуры
        List<Figure> colorFigures = GetFiguresByColor(color);
        // Получаем список клеток под боем
        List<Vector2Int> allCellsUnderAttack = colorFigures.SelectMany(figure => figure.GetCellsUnderAttack()).ToList();
        return allCellsUnderAttack;
    }

    /// <summary>
    /// Получает список разрешенных (не приводящих к шаху) ходов фигур того цвета, который сейчас ходит.
    /// </summary>
    public List<FigureMove> GetLegalMoves()
    {
        // Список обновляется после каждого хода в методе UpdateLegalMoves
        return legalMoves;
    }

    /// <summary>
    /// Обновляет список разрешенных (не приводящих к шаху) ходов фигур того цвета, который сейчас ходит.
    /// </summary>
    public void UpdateLegalMoves()
    {
        legalMoves.Clear();
        // Получаем список возможных ходов
        List<FigureMove> ownMoves = GetMovesByColor(turnColor, special: true);
        // Пытаемся сделать ход на виртуальной доске
        foreach(FigureMove ownMove in ownMoves)
        {
            // Если ход уже отмечен как запрещенный, его можно не проверять
            if(!ownMove.passedFirstCheck)
            {
                continue;
            }
            // Создаем виртуальную доску
            BoardState virtualBoard = new BoardState(this);
            // Двигаем фигуру
            Figure figure = virtualBoard.GetFigureAtCell(ownMove.from);
            figure.ExecuteMove(ownMove);
            // Если ход не приводит к шаху, то он разрешен
            if(!virtualBoard.DetectCheck(turnColor))
            {
                legalMoves.Add(ownMove);
            }
        }
    }

    /// <summary>
    /// Совершить ход.
    /// </summary>
    public void ExecuteMove(FigureMove move)
    {
        Figure figure = GetFigureAtCell(move.from);
        if(figure == null)
        {
            throw new InvalidOperationException("Невозможно совершить ход: не найдена фигура");
        }
        figure.ExecuteMove(move);
    }

    /// <summary>
    /// Сохранить в файл.
    /// </summary>
    public void Serialize(string filename)
    {
        var settings = new XmlWriterSettings {
            Indent = true,
            IndentChars = "    "
        };
        XmlWriter writer = XmlWriter.Create(filename, settings);
        DataContractSerializer ser = new DataContractSerializer(typeof(BoardState));
        ser.WriteObject(writer, this);
        writer.Close();
    }

    /// <summary>
    /// Загрузить из файла.
    /// </summary>
    public static BoardState Deserialize(string filename, Action<Figure> figureCreatedCallback = null)
    {
        FileStream fs = new FileStream(filename, FileMode.Open);
        XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());
        DataContractSerializer ser = new DataContractSerializer(typeof(BoardState));
        BoardState deserializedObject = (BoardState)ser.ReadObject(reader, true);
        reader.Close();
        fs.Close();

        // Временный список legalMoves не сохраняется в файл, поэтому его необходимо создать
        deserializedObject.legalMoves = new List<FigureMove>();
        deserializedObject.UpdateLegalMoves();
        // Коллбек необходимо привязать заново
        deserializedObject.figureCreatedCallback = figureCreatedCallback;

        return deserializedObject;
    }

}
