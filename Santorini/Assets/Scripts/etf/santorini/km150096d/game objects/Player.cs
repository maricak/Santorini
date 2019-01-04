using UnityEngine;

using etf.santorini.km150096d.moves;
using etf.santorini.km150096d.utils;
using System;

namespace etf.santorini.km150096d.model
{
    public enum PlayerID : int { PLAYER0 = 0, PLAYER1 };
    public enum PlayerType : int { HUMAN, EASY, MEDIUM, HARD };

    public class Player : MonoBehaviour
    {
        private static readonly Player[,] players = new Player[2, 2];
        public static Player selectedPlayer;

        public static PlayerID turn = PlayerID.PLAYER0;

        // move strategies
        private static Move[] moves = playerMoves;
        private static readonly Move[] fileMoves = new Move[2];
        private static readonly Move[] playerMoves = new Move[2];

        private static bool readigFromFileInProgres;

        public PlayerID Id { get; set; }
        public Vector2 Position { get; set; }


        // initialize players' logic ----> add later
        public static void Init(Board board, PlayerType type1, PlayerType type2, bool simulation, int maxDepth, bool fileLoaded)
        {
            fileMoves[0] = new FileMove(PlayerID.PLAYER0, board);
            fileMoves[1] = new FileMove(PlayerID.PLAYER1, board);

            playerMoves[0] = CreateMove(type1, PlayerID.PLAYER0, board, maxDepth);
            playerMoves[1] = CreateMove(type2, PlayerID.PLAYER1, board, maxDepth);

            moves = playerMoves;

            if(fileLoaded)
            {
                StartReadingFromFile();
            }
        }

        private static Move CreateMove(PlayerType type1, PlayerID id, Board board, int maxDepth)
        {
            switch(type1)
            {
                case PlayerType.HUMAN:
                    return new HumanMove(id, board);
                case PlayerType.EASY:
                    return new AISimpleMove(id, board, maxDepth);
            }
            // TODO dodati!

            return new HumanMove(id, board);
        }

        #region ReadingFromFile
        public static void StartReadingFromFile()
        {
            readigFromFileInProgres = true;
            moves = fileMoves;
        }
        public static bool ReadingInProgress()
        {
            return readigFromFileInProgres;
        }
        public static void FinishReadingFromFile()
        {
            readigFromFileInProgres = false;
            moves = playerMoves;
            moves[0].CopyMove(fileMoves[0]);
            moves[1].CopyMove(fileMoves[1]);
        }
        #endregion

        #region Player
        public static void GeneratePlayer(PlayerID id, Vector2 position, Board board, int index)
        {
            // position
            int x = (int)position.x;
            int y = (int)position.y;

            Tile tile = Tile.GetTile(x, y);
            // create object
            GameObject gameObject = Instantiate(board.playerPrefabs[(int)id]) as GameObject;
            gameObject.transform.SetParent(board.transform);
            Player player = gameObject.GetComponent<Player>();

            // init object
            player.Id = id;
            player.Position = position;
            players[(int)id, index] = player;
            tile.Player = player;

            // set position
            Util.MovePlayer(player, x, y, 0);
        }
        public static Player GetPlayer(PlayerID playerId, int index)
        {
            return players[(int)playerId, index];
        }
        #endregion


        #region Game
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
        #endregion

        #region Moves
        public static void MakeMove(Vector2 position)
        {
            moves[(int)turn].MakeMove(position);
        }

        public static bool HasPossibleMoves()
        {
            return moves[(int)turn].HasPossibleMoves();
        }

        public static bool MouseInputNeeded()
        {
            return moves[(int)turn].MouseInputNeeded();
        }
        #endregion
    }
}