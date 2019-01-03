using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum MoveState { SELECT, MOVE, BUILD, FINISH };

public class Move
{
    private static Move instance = null;
    protected Move() { }
    public static Move Instance
    {
        get
        {
            if (instance == null)
                instance = new Move();
            return instance;
        }
    }
    public bool PositioningInProgress { get; set; }

    private bool[,] possibleMoves = new bool[Board.DIM, Board.DIM];

    private int[] moveCount = { 0, 0 }; // because every move consists of two moves
    private MoveState moveState = MoveState.SELECT;

    public void Position(PlayerType type, Vector2 position, Board board)
    {
        // generate player
        Player.GeneratePlayer(type, position, board, moveCount[(int)type]);
        moveCount[(int)type]++;

        // check if player positionin is over
        if (moveCount[(int)type] == 2)
        {
            FileManager.Instance.WritePositions(type);
            ResetMoveCount(type);
            // finish positioning 
            if (type == PlayerType.PLAYER1)
            {
                FinishPositioningPlayers();
            }
            // change turn
            Player.ChangeTurn();
        }
    }

    public void StartPositioningPlayers()
    {
        PositioningInProgress = true;
    }
    public void FinishPositioningPlayers()
    {
        PositioningInProgress = false;
    }
    public void ResetMoveCount(PlayerType type)
    {
        moveCount[(int)type] = 0;
    }
    /* 
        move contains three clicks
        1. select player
        2. move player
        3. build
    */
    public void MakeMove(PlayerType playerType, Vector3 position, Board board)
    {
        switch (moveState)
        {
            case MoveState.SELECT:
                if (SelectPlayer(playerType, position))
                {
                    moveState = MoveState.MOVE;
                    FileManager.Instance.Src = position;
                }
                break;
            case MoveState.MOVE:
                if (MovePlayer(playerType, position)) // move selected
                {
                    moveState = MoveState.BUILD;
                    FileManager.Instance.Dst = position;
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
                    FileManager.Instance.Build =position;
                }
                break;
            default: break;
        }
        if (moveState == MoveState.FINISH)
        {
            FileManager.Instance.WriteMove();
            moveState = MoveState.SELECT;
            Player.ChangeTurn();
        }
    }


    private bool SelectPlayer(PlayerType playerType, Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        Tile selectedTile = Tile.GetTile(x, y);
        if (selectedTile.HasPlayer() && selectedTile.Player.Type == playerType)
        {
            Player.selectedPlayer = selectedTile.Player;

            // highlight
            CalculatePossibleMoves(Player.selectedPlayer.Position);
            Highlight.SetHighlight(possibleMoves);

            return true;
        }
        return false;
    }

    private bool MovePlayer(PlayerType playerType, Vector2 dstPosition)
    {
        Highlight.ResetHighlight(possibleMoves);

        int x = (int)dstPosition.x;
        int y = (int)dstPosition.y;

        Player player = Player.selectedPlayer;
        Tile tileSrc = Tile.GetTile((int)Player.selectedPlayer.Position.x, (int)Player.selectedPlayer.Position.y);
        Tile tileDst = Tile.GetTile(x, y);

        if (possibleMoves[x, y])
        {
            // remove player from old tile
            tileSrc.Player = null;

            // move player to new tile
            tileDst.Player = player;
            Player.selectedPlayer.Position = dstPosition;
            Util.MovePlayer(player, x, y, (int)(tileDst.Height - 1));


            CalculatePossibleBuilds(dstPosition);
            if (!CheckMoves())
            {
                return false;
            }
            Highlight.SetHighlight(possibleMoves);
            return true;
        }
        else
        {
            Player.selectedPlayer = null;
            return false;
        }
    }

    private bool Build(Vector2 position, Board board)
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

    public bool HasPossibleMoves(PlayerType playerType)
    {
        if (moveState == MoveState.SELECT) // naredni klik je select
        {
            for (int k = 0; k < 2; k++)
            {
                Player player = Player.GetPlayer(playerType, k);
                int x = (int)player.Position.x;
                int y = (int)player.Position.y;
                Tile playerTile = Tile.GetTile(x, y);

                for (int i = 0; i < Board.DIM; i++)
                {
                    for (int j = 0; j < Board.DIM; j++)
                    {
                        if (i == x && j == y)
                        {
                            continue;
                        }
                        else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                        {
                            Tile tile = Tile.GetTile(i, j);
                            if (tile.HasPlayer() || tile.Height == Height.ROOF || (playerTile.Height + 1).CompareTo(tile.Height) < 0)
                            {
                                continue;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        else if (moveState == MoveState.BUILD) // naredni klik je build
        {
            int x = (int)Player.selectedPlayer.Position.x;
            int y = (int)Player.selectedPlayer.Position.y;
            for (int i = 0; i < Board.DIM; i++)
            {
                for (int j = 0; j < Board.DIM; j++)
                {
                    if (i == x && j == y)
                    {
                        continue;
                    }
                    else if (Math.Abs(x - i) <= 1 && Math.Abs(j - y) <= 1)
                    {
                        Tile tile = Tile.GetTile(i, j);
                        if (!tile.HasPlayer() && (tile.Height != Height.ROOF))
                            return true;
                    }
                }
            }
            return false;
        }
        return true;
    }
    private void CalculatePossibleMoves(Vector2 playerPosition)
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
                }
                else
                {
                    possibleMoves[i, j] = false;
                }
            }
        }
    }

    private void CalculatePossibleBuilds(Vector2 playerPosition)
    {
        int x = (int)playerPosition.x;
        int y = (int)playerPosition.y;

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
    private bool CheckMoves()
    {
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
}
