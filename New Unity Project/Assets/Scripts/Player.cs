using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType : int { PLAYER0 = 0, PLAYER1 };

public class Player : MonoBehaviour
{
    /*public static GameObject[] playerPrefabs = new GameObject[2];
    public  GameObject[] playerPrefabsTmp = new GameObject[2];
    */
    public static Player selectedPlayer;
    public static Vector2 selectedPosition;

    public static PlayerType turn = PlayerType.PLAYER0;

    public PlayerType Type { get; set; }

    public static void GeneratePlayer(PlayerType type, Vector3 mouseOver, Board board)
    {
        // position
        int x = (int)mouseOver.x;
        int y = (int)mouseOver.y;

        Tile tile = Tile.GetTile(x, y);
        // create object
        GameObject gameObject = Instantiate(board.playerPrefabs[(int)type]) as GameObject;
        gameObject.transform.SetParent(board.transform);
        Player player = gameObject.GetComponent<Player>();
        player.Type = type;

        tile.Player = player;

        // set position
        Util.MovePlayer(player, x, y, 0);
    }

    public static void ChangeTurn()
    {
        turn = 1 - turn;
    }

    public static bool IsWinner()
    {
        Tile tile = Tile.GetTile((int)selectedPosition.x, (int)selectedPosition.y);
        if (tile.Height == Height.H3)
        {
            return true;
        }
        return false;
    }
}
