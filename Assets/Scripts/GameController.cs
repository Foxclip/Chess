using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public GameObject board;

    // Start is called before the first frame update
    void Start()
    {
        // Создание объектов для клеток
        string letters = "abcdefgh";
        for (int x = 0; x < 8; x++)
        {
            for (int y = 1; y <= 8; y++)
            {
                GameObject newCell = new GameObject(letters[x].ToString() + y.ToString());
                newCell.transform.parent = board.transform.Find("cells");
                float halfCell = (float)(1.0 / 16.0);
                float cellX = (float)(x / 8.0) + halfCell;
                float cellY = (float)(y / 8.0) - halfCell;
                newCell.transform.localPosition = new Vector3(cellX, cellY);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
