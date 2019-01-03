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
        private static Move[] moves;
        private static readonly Move[] fileMoves = new Move[2];
        private static readonly Move[] playerMoves = new Move[2];

        private static bool readigFromFileInProgres;

        public PlayerType Type { get; set; }
        public Vector2 Position { get; set; }


        // initialize players' logic ----> add later
        public static void Init(Board board)
        {
            fileMoves[0] = new FileMove(PlayerType.PLAYER0, board);
            fileMoves[1] = new FileMove(PlayerType.PLAYER1, board);
            playerMoves[0] = new HumanMove(PlayerType.PLAYER0, board);
            playerMoves[1] = new HumanMove(PlayerType.PLAYER1, board);

            moves = fileMoves;
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
        public static Player GetPlayer(PlayerType playerType, int index)
        {
            return players[(int)playerType, index];
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