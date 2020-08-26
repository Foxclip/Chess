using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceController : MonoBehaviour
{

    private static GameObject selectedPiece = null;
    private static Transform[] legalMoveCells;

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
        Debug.Log($"PRESSED {gameObject.name}");
        selectedPiece = gameObject;
    }

    public static void BoardClick()
    {
        selectedPiece = null;
        if(legalMoveCells != null)
        {
            foreach(Transform obj in legalMoveCells)
            {
                Destroy(obj);
            }
        }
    }

}
