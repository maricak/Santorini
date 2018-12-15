using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Collections;

public class Board : MonoBehaviour
{
    public static Board Instance { get; set; }
    public static readonly int DIM = 5;

    public GameObject tilePrefab;
    public GameObject[] playerPrefabs = new GameObject[2];
    public GameObject blockPrefab;
    public GameObject highlightPrefab;
    public GameObject roofPrefab;


    // position of the mouse
    private static Vector2 mouseOver;
    private static bool gameOver;
    private static PlayerType winner;


    private void Start()
    {
        Instance = this;
        // initialize mouse position
        Board.ResetMouseOver();
        // generate board tiles
        Board.GenerateBoard();

        // position builders for both players
        Move.StartPositioningPlayers();
    }


    private void Update()
    {
        // update mouse postion
        UpdateMouseOver();
        if (Input.GetMouseButtonDown(0) && MouseInsideBoard() && !gameOver)
        {
            if (Move.positioningInProgress)
            {
                Move.Position(Player.turn, mouseOver, Board.Instance);
            }
            else
            {
                Move.MakeMove(Player.turn, mouseOver, Board.Instance);
                CheckGameOver();
            }
        }
    }

    #region MouseOver
    // update the position of the mouse
    private static void UpdateMouseOver()
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
            //Debug.Log("double x=" + x + "double y=" + y);
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
    private static void ResetMouseOver()
    {
        mouseOver.x = -1;
        mouseOver.y = -1;
    }
    // check if mouse position is inside the board
    private static bool MouseInsideBoard()
    {
        return mouseOver.x >= 0 && mouseOver.x < DIM && mouseOver.y >= 0 && mouseOver.y < DIM;
    }
    #endregion

    #region GenerateBoard
    private static void GenerateBoard()
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

    private static void CheckGameOver()
    {
        if (!Move.HasPossibleMoves(mouseOver))
        {
            gameOver = true;
            winner = 1 - Player.turn;
        } else if(Player.IsWinner())
        {
            gameOver = true;
            winner = Player.turn;
        }
    }

    #endregion

}
