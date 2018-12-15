using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Collections;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private static Board Instance { get; set; }
    private Board() { }
    public static readonly int DIM = 5;

    public GameObject tilePrefab;
    public GameObject[] playerPrefabs = new GameObject[2];
    public GameObject blockPrefab;
    public GameObject highlightPrefab;
    public GameObject roofPrefab;

    public Canvas messageCanvas;


    // position of the mouse
    private Vector2 mouseOver;
    private bool gameOver = false;
    private PlayerType winner;

    private void Start()
    {
        Instance = this;
        FileManager.Instance.CreateFile();        
        // initialize mouse position
        ResetMouseOver();
        // generate board tiles
        GenerateBoard();

        // position builders for both players
        Move.Instance.StartPositioningPlayers();

        UpdateMessage("Turn: " + Player.turn);

    }

    private void Update()
    {
        // update mouse postion
        UpdateMouseOver();
        if (Input.GetMouseButtonDown(0) && MouseInsideBoard() && !gameOver)
        {
            if (Move.Instance.PositioningInProgress)
            {
                Move.Instance.Position(Player.turn, mouseOver, Board.Instance);
            }
            else
            {
                Move.Instance.MakeMove(Player.turn, mouseOver, Board.Instance);
                CheckGameOver();
            }
        }
    }

    private void OnApplicationQuit()
    {
        FileManager.Instance.SaveFile();
    }


    #region MouseOver
    // update the position of the mouse
    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            // calculate tile index
            var x = (hit.point.x / (Tile.DISTANCE + Tile.SIZE));
            var y = (hit.point.z / (Tile.DISTANCE + Tile.SIZE));
            if (x % 1 <= 0.9 && y % 1 <= 0.9)
            {
                mouseOver.x = (int)x;
                mouseOver.y = (int)y;
            }
            else
            {
                ResetMouseOver();
            }
        }
        else
        {
            ResetMouseOver();
        }
    }
    private void ResetMouseOver()
    {
        mouseOver.x = -1;
        mouseOver.y = -1;
    }
    // check if mouse position is inside the board
    private bool MouseInsideBoard()
    {
        return mouseOver.x >= 0 && mouseOver.x < DIM && mouseOver.y >= 0 && mouseOver.y < DIM;
    }
    #endregion

    #region GenerateBoard
    private void GenerateBoard()
    {
        for (int x = 0; x < DIM; x++)
        {
            for (int y = 0; y < DIM; y++)
            {
                Tile.GenerateTile(x, y, Board.Instance);
                Highlight.GenerateHighlight(x, y, Board.Instance);
            }
        }
    }
    #endregion

    #region 

    private void CheckGameOver()
    {
        if (!Move.Instance.HasPossibleMoves(Player.turn))
        {
            gameOver = true;
            winner = 1 - Player.turn;
            UpdateMessage("Winner is " + winner + "!");
        }
        else if(Player.IsWinner())
        {
            gameOver = true;
            winner = Player.turn;
            UpdateMessage("Winner is " + winner + "!");
        }
    }

    #endregion

    public static void UpdateMessage(string message)
    {
        Instance.messageCanvas.GetComponentInChildren<Text>().text = message;
    }

}
