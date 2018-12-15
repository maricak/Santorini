using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType : int { PLAYER0 = 0, PLAYER1 };

public class Player : MonoBehaviour
{
    private static Player[,] players = new Player[2, 2];
    public static Player selectedPlayer;

    public static PlayerType turn = PlayerType.PLAYER0;

    public PlayerType Type { get; set; }

    public Vector2 Position { get; set; }

    public static Player GetPlayer(PlayerType type, int index)
    {
        return players[(int)type, index];
    }
    public static void GeneratePlayer(PlayerType type, Vector2 position, Board board, int index)
    {
        // position
        int x = (int)position.x;
        int y = (int)position.y;

        Tile tile = Tile.GetTile(x, y);
        // create object
        GameObject gameObject = Instantiate(board.playerPrefabs[(int)type]) as GameObject;
        gameObject.transform.SetParent(board.transform);
        Player player = gameObject.GetComponent<Player>();
        player.Type = type;
        players[(int)type, index] = player;
        tile.Player = player;

        // set position
        Util.MovePlayer(player, x, y, 0);
        player.Position = position;
    }

    public static void ChangeTurn()
    {
        turn = 1 - turn;
        Board.UpdateMessage("Turn: " + turn);
    }

    public static bool IsWinner()
    {
        if (!selectedPlayer)
        {
            return false;
        }
        Tile tile = Tile.GetTile((int)selectedPlayer.Position.x, (int)selectedPlayer.Position.y);
        if (tile.Height == Height.H3)
        {
            return true;
        }
        return false;
    }
}
