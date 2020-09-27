using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IllegalMoveController : MonoBehaviour
{
    private GameController gameController;

    /// <summary>
    /// Привязанная фигура.
    /// </summary>
    public Transform piece;
    /// <summary>
    /// Привязанный ход на BoardState
    /// </summary>
    public FigureMove move;

    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
    }

    public void ObjectClicked()
    {
        if(gameController.interfaceLocked)
        {
            return;
        }

        // Получаем фигуры, атакующие клетку
        Vector2Int cell = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        Figure.FigureColor enemyColor = Figure.InvertColor(gameController.boardState.turnColor);
        List<Figure> attackingFigures = move.attackingFigures;

        King king = gameController.boardState.FindKingByColor(gameController.boardState.turnColor);
        ShakeAnimation shakeAnimation = gameController.FindTransformByPos(king.Pos).GetComponent<ShakeAnimation>();
        Figure boundFigure = gameController.boardState.GetFigureAtCell(move.from);

        foreach(Figure figure in attackingFigures)
        {
            // Добавляем объект
            GameObject newRedLine = Instantiate(gameController.redLine);
            gameController.redLines.Add(newRedLine);
            // Если это ход короля, то анмация тряски не нужна
            Action callback;
            if(boundFigure.GetType() == typeof(King))
            {
                callback = null;
            }
            else
            {
                callback = shakeAnimation.StartAnimation;
            }
            // Запускаем анимацию
            RedLineAnimation redLineAnimation = newRedLine.GetComponent<RedLineAnimation>();
            redLineAnimation.StartAnimation(
                beginPos: new Vector3(figure.Pos.x, figure.Pos.y),
                endPos: new Vector3(move.kingPos.x, move.kingPos.y),
                finishedCallback: callback
            );
        }
    }
}
