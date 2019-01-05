using System.IO;
using UnityEngine;
using UnityEngine.UI;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.menu;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.moves;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Board : MonoBehaviour
    {
        #region Singleton
        public static Board Instance { get; set; }
        private Board() { }
        #endregion

        public static readonly int DIM = 5;

        #region Prefabs and canvas
        public GameObject tilePrefab;
        public GameObject[] playerPrefabs = new GameObject[2];
        public GameObject blockPrefab;
        public GameObject highlightPrefab;
        public GameObject roofPrefab;

        // message
        public Canvas messageCanvas;
        #endregion

        #region Tiles
        private readonly ITile[,] tiles = new Tile[DIM, DIM];
        public ITile this[int i, int j]
        {
            get { return tiles[i, j]; }
        }
        public ITile this[Vector2 position]
        {
            get { return tiles[(int)position.x, (int)position.y]; }
        }
        #endregion

        #region Players
        // TODO modifikatori?
        public readonly IPlayer[,] players = new Player[2, 2];
        public IPlayer selectedPlayer;
        public PlayerID turnId = PlayerID.PLAYER0;

        private readonly Move[] fileMoves = new Move[2];
        private readonly Move[] playerMoves = new Move[2];
        private Move[] moves;

        #endregion

        private static bool readigFromFileInProgres;

        // position of the mouse
        private Vector2 mouseOver;

        #region Game variables
        private bool gameOver = false;
        private PlayerID winner;

        private float deltaTime = 0.0f;
        public float threshold = 0.5f;

        public bool Simulation { set; get; }
        public int MaxDepth { set; get; }
        #endregion

        #region Unity runtime methods
        private void Start()
        {
            Instance = this;

            // initialize files
            FileManager.Instance.SetOutput();

            // initialize mouse position
            ResetMouseOver();
            // generate board tiles
            GenerateBoard();

            InitPlayers(Instance,
              (PlayerType)Menu.Instance.player1.value,
              (PlayerType)Menu.Instance.player2.value,
              Menu.Instance.simulation.isOn,
              Menu.Instance.depth.value + 1,
              Menu.Instance.loadFromFile);

            UpdateMessage("Turn: " + turnId);
        }
        private void Update()
        {
            deltaTime += Time.deltaTime;

            // update mouse postion
            UpdateMouseOver();

            if (!gameOver)
            {
                if (MouseInputNeeded()) // wait for mouse click
                {
                    if (Input.GetMouseButtonDown(0) && MouseInsideBoard())
                    {
                        MakeMove(mouseOver);
                    }
                }
                else if (deltaTime > threshold) // read move from file or ai after treshold time
                {
                    deltaTime = 0.0f;
                    try
                    {
                        // position is not important                    
                        MakeMove(Vector2.zero);
                    }
                    catch (IOException)
                    {
                        UpdateMessage("Input file is corrupted!");
                        FinishReadingFromFile();
                    }
                }
                CheckGameOver();
            }
        }
        private void OnApplicationQuit()
        {
            FileManager.Instance.SaveFile();
        }
        #endregion



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
                var x = hit.point.x / (Tile.DISTANCE + Tile.SIZE);
                var y = hit.point.z / (Tile.DISTANCE + Tile.SIZE);
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
            return mouseOver.x >= 0 && mouseOver.x < DIM && mouseOver.y >= 0 && mouseOver.y < DIM;
        }
        #endregion

        #region GenerateBoard
        private void GenerateBoard()
        {
            for (int x = 0; x < DIM; x++)
            {
                for (int y = 0; y < DIM; y++)
                {
                    tiles[x, y] = Tile.GenerateTile(x, y, Board.Instance);
                    Highlight.GenerateHighlight(x, y, Board.Instance);
                }
            }
        }
        #endregion

        #region Game methods

        private void CheckGameOver()
        {
            // current player has no possible moves
            if (IsGameOver(ref winner))
            {
                gameOver = true;
                UpdateMessage("Winner is " + winner + "!");
            }
        }
        public bool IsGameOver(ref PlayerID winnerId)
        {
            if (moves[(int)PlayerID.PLAYER0].IsWinner() || !moves[(int)PlayerID.PLAYER1].HasPossibleMoves())
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
        public void ChangeTurn()
        {
            turnId = 1 - turnId;
            Board.UpdateMessage("Turn: " + turnId);
        }

        public void MakeMove(Vector2 position)
        {
            moves[(int)turnId].MakeMove(position);
        }

        public bool HasPossibleMoves()
        {
            return moves[(int)turnId].HasPossibleMoves();
        }

        public bool MouseInputNeeded()
        {
            return moves[(int)turnId].MouseInputNeeded();
        }
        #endregion

        #region Message
        public static void UpdateMessage(string message)
        {
            Instance.messageCanvas.GetComponentInChildren<Text>().text = message;
        }
        #endregion

        #region Game init
        public void InitPlayers(Board board, PlayerType type1, PlayerType type2, bool simulation, int maxDepth, bool fileLoaded)
        {
            fileMoves[0] = new FileMove(PlayerID.PLAYER0, board);
            fileMoves[1] = new FileMove(PlayerID.PLAYER1, board);

            playerMoves[0] = CreateMove(type1, PlayerID.PLAYER0, board, maxDepth);
            playerMoves[1] = CreateMove(type2, PlayerID.PLAYER1, board, maxDepth);

            moves = playerMoves;

            if (fileLoaded)
            {
                StartReadingFromFile();
            }
        }
        private Move CreateMove(PlayerType type1, PlayerID id, Board board, int maxDepth)
        {
            switch (type1)
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

        #region Player 
        public Vector2[] GetPlayerPositions(PlayerID playerId)
        {
            Vector2[] positions = new Vector2[2];
            positions[0] = players[(int)playerId, 0].Position;
            positions[1] = players[(int)playerId, 1].Position;
            return positions;
        }
        #endregion

        #region ReadingFromFile
        public void StartReadingFromFile()
        {
            readigFromFileInProgres = true;
            moves = fileMoves;
        }
        public bool ReadingInProgress()
        {
            return readigFromFileInProgres;
        }
        public void FinishReadingFromFile()
        {
            readigFromFileInProgres = false;
            moves = playerMoves;
            moves[0].CopyMove(fileMoves[0]);
            moves[1].CopyMove(fileMoves[1]);
        }
        #endregion

        #region Highlight
        public void SetHighlight(bool[,] possibleMoves)
        {
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    if (possibleMoves[i, j])
                    {
                        (tiles[i, j] as Tile).SetHighlight(i, j);
                    }
                }
            }
        }
        public void ResetHighlight()
        {
            for (int i = 0; i < DIM; i++)
            {
                for (int j = 0; j < DIM; j++)
                {
                    (tiles[i, j] as Tile).ResetHighlght();
                }
            }
        }
        #endregion
    }
}