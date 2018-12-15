using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Collections;

public class Board : MonoBehaviour
{

    private const int BOARD_SIZE = 5;

    public Tile[,] tiles = new Tile[BOARD_SIZE, BOARD_SIZE];
    public GameObject tilePrefab;

    public GameObject[,] highlights = new GameObject[BOARD_SIZE, BOARD_SIZE];
    public GameObject highlightPrefab;

    public GameObject[] playerPrefabs = new GameObject[2];
    public Player[,] players = new Player[2, 2];

    private Player selectedPlayer;
    private Vector2 selectedPosition;

    private int[] moveCount = { 0, 0 }; // because every move consists of two moves

    private bool positioningInProgress = false;

    private int turn = 0;

    private bool[,] possibleMoves = new bool[BOARD_SIZE, BOARD_SIZE];

    private List<Block> blocks = new List<Block>();
    public GameObject blockPrefab;

    // position of the mouse
    private Vector2 mouseOver;


    private void Start()
    {
        // initialize mouse position
        ResetMouseOver();
        // generate board tiles
        GenerateBoard();

        // position builders for both players
        StartPositioningPlayers();
    }


    private void Update()
    {
        // update mouse postion
        UpdateMouseOver();
        if (Input.GetMouseButtonDown(0) && MouseInsideBoard())
        {
            if (positioningInProgress)
            {
                PositionPlayer(turn);
            }
            else
            {
                MakeMove(turn);
            }
        }
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
    private void ResetMouseOver()
    {
        mouseOver.x = -1;
        mouseOver.y = -1;
    }
    // check if mouse position is inside the board
    private bool MouseInsideBoard()
    {
        return mouseOver.x >= 0 && mouseOver.x < BOARD_SIZE && mouseOver.y >= 0 && mouseOver.y < BOARD_SIZE;
    }
    #endregion

    #region GenerateBoard
    private void GenerateBoard()
    {
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                GenerateTile(x, y);
                GenerateHighlight(x, y);
            }
        }
    }

    private void GenerateTile(int x, int y)
    {
        GameObject gameObject = Instantiate(tilePrefab) as GameObject;
        gameObject.transform.SetParent(transform);
        Tile tile = gameObject.GetComponent<Tile>();
        tiles[x, y] = tile;
        tile.transform.position = Vector3.right * (x + x * Tile.DISTANCE) + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
    }
    private void GenerateHighlight(int x, int y)
    {
        GameObject highlight = Instantiate(highlightPrefab) as GameObject;
        highlight.transform.SetParent(transform);
        highlights[x, y] = highlight;
        highlight.transform.position = Vector3.right * (x + x * Tile.DISTANCE) +
            -Vector3.down * (int)(tiles[x, y].Height) * 0.11f
            + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
        highlight.SetActive(false);
    }
    #endregion

    #region PositionPlayers
    private void StartPositioningPlayers()
    {
        positioningInProgress = true;
    }
    private void FinishPositioningPlayers()
    {
        positioningInProgress = false;
    }
    private void PositionPlayer(int playerType)
    {
        // generate player
        GeneratePlayer(playerType);
        moveCount[playerType]++;

        // check if player positionin is over
        if (moveCount[playerType] == 2)
        {
            ResetMoveCount(playerType);
            // finish positioning 
            if (playerType == 1)
            {
                FinishPositioningPlayers();
            }
            // change turn
            this.turn = 1 - this.turn;
        }
    }
    private void GeneratePlayer(int playerType)
    {
        // position
        int x = (int)mouseOver.x;
        int y = (int)mouseOver.y;
        int index = moveCount[playerType];
        // create object
        GameObject gameObject = Instantiate(playerPrefabs[playerType]) as GameObject;
        gameObject.transform.SetParent(transform);
        Player player = gameObject.GetComponent<Player>();
        player.Type = playerType;

        players[playerType, index] = player;
        tiles[x, y].Player = player;

        // set position
        MovePlayer(player, x, y);
    }

    private void MovePlayer(Player player, int x, int y)
    {
        player.transform.position = Vector3.right * (x + x * Tile.DISTANCE) - Vector3.down * 0.45f + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
    }
    #endregion

    #region Movement
    private void ResetMoveCount(int index)
    {
        moveCount[index] = 0;
    }

    /* 
        move contains three clicks
        1. select player
        2. move player
        3. build
    */
    private void MakeMove(int playerType)
    {

        Debug.Log("MakaMove + player: " + playerType + "move: " + moveCount[playerType]);
        switch (moveCount[playerType])
        {
            case 0:
                if (SelectPlayer(playerType))
                {
                    moveCount[playerType]++;
                }
                break;
            case 1:
                if (MovePlayer()) // move selected
                {
                    moveCount[playerType]++;
                }
                break;
            case 2:
                if (Build())
                {
                    moveCount[playerType]++;
                }
                break;
            default: break;
        }
        if (moveCount[playerType] == 3)
        {
            ResetMoveCount(playerType);
            turn = 1 - turn;
        }
    }

    private bool SelectPlayer(int playerType)
    {
        int x = (int)mouseOver.x;
        int y = (int)mouseOver.y;
        if (tiles[x, y].HasPlayer() && tiles[x, y].Player.Type == playerType)
        {
            selectedPlayer = tiles[x, y].Player;
            selectedPosition = mouseOver;

            // highlight
            CalculatePossibleTurns();
            SetHighlight();

            return true;
        }

        return false;
    }

    private bool MovePlayer()
    {
        ResetHighlight();

        int x = (int)mouseOver.x;
        int y = (int)mouseOver.y;

        if (possibleMoves[x, y])
        {
            // remove player from old tile
            tiles[(int)selectedPosition.x, (int)selectedPosition.y].Player = null;

            // move player to new tile
            tiles[x, y].Player = selectedPlayer;
            selectedPlayer.gameObject.transform.position = Vector3.right * (x + x * Tile.DISTANCE) +
                    -Vector3.down * (int)(tiles[x, y].Height - 1) * 0.10f
                    + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET
                    - Vector3.down * 0.45f;
            selectedPosition.x = x;
            selectedPosition.y = y;
            CalculatePossibleTurns();
            SetHighlight();
            return true;
        }
        else
        {
            selectedPlayer = null;
            selectedPosition = Vector2.zero;
            return false;
        }
    }

    private bool Build()
    {
        ResetHighlight();
        int x = (int)mouseOver.x;
        int y = (int)mouseOver.y;

        if (possibleMoves[x, y])
        {
            if (tiles[x, y].Height != Height.ROOF)
            {
                GenerateBlock(x, y);
                tiles[x, y].Height++;
                return true;
            }
        }
        return false;
    }
    private void GenerateBlock(int x, int y)
    {
        GameObject gameObject = Instantiate(blockPrefab) as GameObject;
        gameObject.transform.SetParent(transform);
        Block block = gameObject.GetComponent<Block>();
        blocks.Add(block);
        block.transform.position = Vector3.right * (x + x * Tile.DISTANCE) +
           -Vector3.down * (int)(tiles[x, y].Height) * 0.10f
           + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
    }

    private void CalculatePossibleTurns()
    {
        int x = (int)selectedPosition.x;
        int y = (int)selectedPosition.y;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (i == x && j == y)
                {
                    possibleMoves[i, j] = false;
                }
                else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                {
                    if (!tiles[i, j].HasPlayer() && tiles[i, j].Height != Height.ROOF && tiles[x, y].Height + 1 >= tiles[i, j].Height)
                        possibleMoves[i, j] = true;
                }
                else
                {
                    possibleMoves[i, j] = false;
                }
            }
        }
    }

    #endregion

    #region Highlight

    private void SetHighlight()
    {
        for (int x = 0; x < BOARD_SIZE; x++)
        {

            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (possibleMoves[x, y])
                {
                    highlights[x, y].transform.position = Vector3.right * (x + x * Tile.DISTANCE) +
           - Vector3.down * (int)(tiles[x, y].Height) * 0.10f
           + Vector3.forward * (y + y * Tile.DISTANCE) + Tile.OFFSET;
                    highlights[x, y].SetActive(true);
                }
            }
        }
    }
    private void ResetHighlight()
    {
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (possibleMoves[x, y])
                {
                    highlights[x, y].SetActive(false);
                }
            }
        }
    }
    #endregion

}
