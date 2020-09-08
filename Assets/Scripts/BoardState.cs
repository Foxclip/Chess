using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Figure
{
    public int x;
    public int y;
    public GameObject gameObject;
    public string color;

    public Figure(int x, int y, string color)
    {
        this.x = x;
        this.y = y;
        this.color = color;
        
    }
    public abstract void Move(int x, int y);
    public abstract List<(int, int)> GetMoveCells();
    public void LoadGameObject(string name)
    {
        gameObject = Object.Instantiate(Resources.Load(name)) as GameObject;
        gameObject.transform.position = new Vector3(x, y);
        gameObject.transform.parent = GameObject.Find("pieces").transform;
    }
}

public class Pawn: Figure
{
    public Pawn(int x, int y, string color, bool createGameObject = false): base(x, y, color)
    {
        if(createGameObject)
        {
            LoadGameObject($"{color}_pawn");
        }
    }

    public override void Move(int x, int y)
    {

    }

    public override List<(int, int)> GetMoveCells()
    {
        return new List<(int, int)>();
    }

}

public class BoardState
{

    List<Figure> figures = new List<Figure>();
    Figure[,] board = new Figure[8, 8];

    public BoardState()
    {
        // Создание белых пешек
        for(int x = 0; x < 8; x++)
        {
            Pawn newPawn = new Pawn(x, 1, color: "white", createGameObject: true);
            figures.Add(newPawn);
            board[newPawn.x, newPawn.y] = newPawn;
        }

    }

}
