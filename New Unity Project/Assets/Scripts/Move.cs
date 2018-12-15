using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum MoveState { SELECT, MOVE, BUILD, FINISH };

public class Move : MonoBehaviour
{

    public static bool positioningInProgress = false;

    private static bool[,] possibleMoves = new bool[Board.DIM, Board.DIM];

    private static readonly int[] moveCount = { 0, 0 }; // because every move consists of two moves
    private static MoveState moveState = MoveState.SELECT;

    public static void Position(PlayerType type, Vector3 mouseOver, Board board)
    {
        // generate player
        Player.GeneratePlayer(type, mouseOver, board);
        moveCount[(int)type]++;

        // check if player positionin is over
        if (moveCount[(int)type] == 2)
        {
            ResetMoveCount(type);
            // finish positioning 
            if (type == PlayerType.PLAYER1)
            {
                FinishPositioningPlayers();
            }
            // change turn
            Player.turn = 1 - Player.turn;
        }
    }

    public static void StartPositioningPlayers()
    {
        positioningInProgress = true;
    }
    public static void FinishPositioningPlayers()
    {
        positioningInProgress = false;
    }
    public static void ResetMoveCount(PlayerType type)
    {
        moveCount[(int)type] = 0;
    }
    /* 
        move contains three clicks
        1. select player
        2. move player
        3. build
    */
    public static void MakeMove(PlayerType playerType, Vector3 position, Board board)
    {
        switch (moveState)
        {
            case MoveState.SELECT:
                if (SelectPlayer(playerType, position))
                {
                    moveState = MoveState.MOVE;
                }
                break;
            case MoveState.MOVE:
                if (MovePlayer(playerType, position)) // move selected
                {
                    moveState = MoveState.BUILD;
                }
                else
                {
                    moveState = MoveState.SELECT;
                }
                break;
            case MoveState.BUILD:
                if (Build(position, board))
                {
                    moveState = MoveState.FINISH;
                    Highlight.ResetHighlight(possibleMoves);
                }
                break;
            default: break;
        }
        if (moveState == MoveState.FINISH)
        {
            moveState = MoveState.SELECT;
            Player.ChangeTurn();
        }
    }


    private static bool SelectPlayer(PlayerType playerType, Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        Tile selectedTile = Tile.GetTile(x, y);
        if (selectedTile.HasPlayer() && selectedTile.Player.Type == playerType)
        {
            Player.selectedPlayer = selectedTile.Player;
            Player.selectedPosition = position;

            // highlight
            CalculatePossibleMoves(Player.selectedPosition);
            Highlight.SetHighlight(possibleMoves);

            return true;
        }
        return false;
    }

    private static bool MovePlayer(PlayerType playerType, Vector2 dstPosition)
    {
        Highlight.ResetHighlight(possibleMoves);

        int x = (int)dstPosition.x;
        int y = (int)dstPosition.y;


        Player player = Player.selectedPlayer;
        Tile tileSrc = Tile.GetTile((int)Player.selectedPosition.x, (int)Player.selectedPosition.y);
        Tile tileDst = Tile.GetTile(x, y);

        if (possibleMoves[x, y])
        {
            // remove player from old tile
            tileSrc.Player = null;

            // move player to new tile
            tileDst.Player = player;
            Player.selectedPosition = dstPosition;
            Util.MovePlayer(player, x, y, (int)(tileDst.Height - 1));

            //
            CalculatePossibleBuilds(dstPosition);
            Highlight.SetHighlight(possibleMoves);
            return true;
        }
        else
        {
            Player.selectedPlayer = null;
            Player.selectedPosition = Vector2.zero;
            return false;
        }
    }

    private static bool Build(Vector2 position, Board board)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        Tile tile = Tile.GetTile(x, y);

        if (possibleMoves[x, y])
        {
            tile.Height++;
            if (tile.Height == Height.ROOF)
            {
                Roof.GenerateRoof(x, y, board);
            }
            else
            {
                Block.GenerateBlock(x, y, board);
            }
            return true;
        }
        return false;

    }

    public static bool HasPossibleMoves(Vector2 playerPosition)
    {
        int x = (int)playerPosition.x;
        int y = (int)playerPosition.y;

        Tile playerTile = Tile.GetTile(x, y);

        for (int i = 0; i < Board.DIM; i++)
        {
            for (int j = 0; j < Board.DIM; j++)
            {
                if (possibleMoves[i, j])
                    return true;
            }
        }
        return false;
    }
    private static void CalculatePossibleMoves(Vector2 playerPosition)
    {
        int x = (int)playerPosition.x;
        int y = (int)playerPosition.y;

        Tile playerTile = Tile.GetTile(x, y);

        for (int i = 0; i < Board.DIM; i++)
        {
            for (int j = 0; j < Board.DIM; j++)
            {
                if (i == x && j == y)
                {
                    possibleMoves[i, j] = false;
                }
                else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                {
                    Tile tile = Tile.GetTile(i, j);
                    if (tile.HasPlayer() || tile.Height == Height.ROOF || (playerTile.Height + 1).CompareTo(tile.Height) < 0)
                    {
                        possibleMoves[i, j] = false;
                    }
                    else
                    {
                        possibleMoves[i, j] = true;
                    }
                    Debug.Log("Compare(" + (playerTile.Height + 1) + ", " + tile.Height + ")=" + (playerTile.Height + 1).CompareTo(tile.Height));
                }
                else
                {
                    possibleMoves[i, j] = false;
                }
            }
        }
    }

    private static void CalculatePossibleBuilds(Vector2 playerPosition)
    {
        int x = (int)playerPosition.x;
        int y = (int)playerPosition.y;

        for (int i = 0; i < Board.DIM; i++)
        {
            //Debug.Log(i + "red: " + tiles[i, 0].Height + tiles[i, 1].Height + tiles[i, 2].Height + tiles[i, 3].Height + tiles[i, 4].Height);
            for (int j = 0; j < Board.DIM; j++)
            {
                if (i == x && j == y)
                {
                    possibleMoves[i, j] = false;
                }
                else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                {
                    Tile tile = Tile.GetTile(i, j);
                    if (!tile.HasPlayer() && (tile.Height != Height.ROOF))
                        possibleMoves[i, j] = true;
                }
                else
                {
                    possibleMoves[i, j] = false;
                }
            }
        }

    }
}
