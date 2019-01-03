using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using etf.santorini.km150096d.moves;

namespace etf.santorini.km150096d
{
    public enum PlayerType : int { PLAYER0 = 0, PLAYER1 };

    public class Player : MonoBehaviour
    {
        private static readonly Player[,] players = new Player[2, 2];
        public static Player selectedPlayer;

        public static PlayerType turn = PlayerType.PLAYER0;

        // move strategies
        public static Move[] moves = new Move[2];

        public static bool PositioningInProgress { get; set; }
        public static bool ReadFromFileInProgres { get; set; }

        public PlayerType Type { get; set; }
        public Vector2 Position { get; set; }


    
      
        // initialize players' logic ----> add later
        public static void Init(Board board)
        {
            moves[0] = new FileMove();
            moves[1] = new FileMove();
            moves[0].Type = PlayerType.PLAYER0;
            moves[1].Type = PlayerType.PLAYER1;
            moves[0].Board = board;
            moves[1].Board = board;
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

            // init object
            player.Type = type;
            player.Position = position;
            players[(int)type, index] = player;
            tile.Player = player;

            // set position
            Util.MovePlayer(player, x, y, 0);
        }

        public static void ChangeTurn()
        {
            turn = 1 - turn;
            Board.UpdateMessage("Turn: " + turn);
        }

        public static bool IsWinner()
        {
            if (selectedPlayer == null)
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

        public static void Move(Vector2 position)
        {
            if (Player.PositioningInProgress)
            {
                Player.PositionPlayer(position);
            }
            else
            {
                Player.MakeMove(position);              
            }
        }

        public static Player GetPlayer(PlayerType playerType, int index)
        {
            return players[(int)playerType, index];
        }

        public static void MakeMove(Vector2 position)
        {
            moves[(int)turn].MakeMove(position);
        }

        public static void PositionPlayer(Vector2 position)
        {
            moves[(int)turn].Position(position);
        }

        public static bool HasPossibleMoves()
        {
            return moves[(int)turn].HasPossibleMoves();
        }

        public static bool MouseInputNeeded()
        {
            return moves[(int)turn].MouseInputNeeded();
        }
    }
}