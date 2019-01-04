using UnityEngine;

using etf.santorini.km150096d.moves;
using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.model.interfaces;

namespace etf.santorini.km150096d.model.gameobject
{


    public class Player : MonoBehaviour, IPlayer
    {
        #region Class fileds
        private static readonly IPlayer[,] players = new Player[2, 2];
        public static IPlayer selectedPlayer;

        public static PlayerID turnId = PlayerID.PLAYER0;

        // move strategies
        private static Move[] moves = playerMoves;
        private static readonly Move[] fileMoves = new Move[2];
        private static readonly Move[] playerMoves = new Move[2];

        private static bool readigFromFileInProgres;
        #endregion

        #region Object fileds
        public PlayerID Id { get; set; }
        public Vector2 Position { get; set; }
        #endregion

        #region Initialisation
        // initialise players' logic ----> add later
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
                    return new AIEasyMove(id, board, maxDepth);
            }
            // TODO dodati!

            return new HumanMove(id, board);
        }
        #endregion

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
        public static IPlayer GetPlayer(PlayerID playerId, int index)
        {
            return players[(int)playerId, index];
        }

        public static Vector2[] GetPlayerPositions(PlayerID playerId)
        {
            Vector2[] positions = new Vector2[2];
            positions[0] = GetPlayer(playerId, 0).Position;
            positions[1] = GetPlayer(playerId, 1).Position;
            return positions;
        }
        #endregion
        
        #region Game
        public static void ChangeTurn()
        {
            turnId = 1 - turnId;
            Board.UpdateMessage("Turn: " + turnId);
        }

        public static bool IsGameOver(ref PlayerID winnerId)
        {
            if(moves[(int)PlayerID.PLAYER0].IsWinner() || !moves[(int)PlayerID.PLAYER1].HasPossibleMoves())
            {
                winnerId = PlayerID.PLAYER0;
                return true;
            }
            else if (moves[(int)PlayerID.PLAYER1].IsWinner() || !moves[(int)PlayerID.PLAYER0].HasPossibleMoves())
            {
                winnerId = PlayerID.PLAYER1;
                return true;
            }
            return false;
        } 
        #endregion

        #region Moves
        public static void MakeMove(Vector2 position)
        {
            moves[(int)turnId].MakeMove(position);
        }

        public static bool HasPossibleMoves()
        {
            return moves[(int)turnId].HasPossibleMoves();
        }

        public static bool MouseInputNeeded()
        {
            return moves[(int)turnId].MouseInputNeeded();
        }
        #endregion
    }
}