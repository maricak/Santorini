using System.IO;
using UnityEngine;
using UnityEngine.UI;

using etf.santorini.km150096d.utils;
using etf.santorini.km150096d.menu;
using etf.santorini.km150096d.model.interfaces;
using etf.santorini.km150096d.moves;

namespace etf.santorini.km150096d.model.gameobject
{
    public class Board : MonoBehaviour, IBoard
    {
        #region Singleton
        public static IBoard Instance { get; set; }
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
        public ITile this[int x, int y]
        {
            get { return tiles[x, y]; }
        }
        public ITile this[Vector2 position]
        {
            get { return tiles[(int)position.x, (int)position.y]; }
        }
        #endregion

        #region Players
        // TODO modifikatori?
        public readonly IPlayer[,] players = new Player[2, 2];
        public IPlayer this[PlayerID id, int num]
        {
            get { return players[(int)id, num]; }
            set { players[(int)id, num] = value; }
        }
        public IPlayer SelectedPlayer { get; set; }
        public PlayerID TurnId { get; set; }

        #endregion

        #region Moves
        private readonly Move[] fileMoves = new Move[2];
        private readonly Move[] playerMoves = new Move[2];
        private Move[] moves;
        #endregion

        // position of the mouse
        private Vector2 mouseOver;

        #region Game variables
        private bool gameOver = false;
        private PlayerID winner;

        private float deltaTime = 0.0f;
        private float threshold = 0.0f;

        public bool Simulation { set; get; }
        public int MaxDepth { set; get; }

        private MoveType moveType0;
        private MoveType moveType1;
        private bool loadFromFile;
        #endregion

        #region Unity runtime methods
        private void Start()
        {
            Instance = this;

            TurnId = PlayerID.PLAYER0;
            // initialize files
            FileManager.Instance.SetOutput();

            // initialize mouse position
            ResetMouseOver();
            // generate board tiles
            GenerateBoard();

            InitMenuInfo();

            InitPlayerMoves();

            SelectedPlayer = players[0, 0];

            UpdateMessage("Turn: " + TurnId);
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
                    tiles[x, y] = Tile.GenerateTile(x, y, this);
                    Highlight.GenerateHighlight(x, y, this);
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
            TurnId = 1 - TurnId;
            UpdateMessage("Turn: " + TurnId);
        }

        public void MakeMove(Vector2 position)
        {
            moves[(int)TurnId].MakeMove(position);
        }

        public bool HasPossibleMoves()
        {
            return moves[(int)TurnId].HasPossibleMoves();
        }

        public bool MouseInputNeeded()
        {
            return moves[(int)TurnId].MouseInputNeeded();
        }
        #endregion

        #region Message
        public void UpdateMessage(string message)
        {
            messageCanvas.GetComponentInChildren<Text>().text = message;
        }
        #endregion

        #region Game init
        public void InitMenuInfo()
        {
            moveType0 = (MoveType)Menu.Instance.player1.value;
            moveType1 = (MoveType)Menu.Instance.player2.value;
            Simulation = Menu.Instance.simulation.isOn;
            MaxDepth = Menu.Instance.depth.value + 1;
            loadFromFile = Menu.Instance.loadFromFile;
        }
        public void InitPlayerMoves()
        {
            if(Simulation)
            {
                threshold = 1f;
            }
            fileMoves[0] = new FileMove(PlayerID.PLAYER0, this);
            fileMoves[1] = new FileMove(PlayerID.PLAYER1, this);

            playerMoves[0] = Move.CreateMove(moveType0, PlayerID.PLAYER0, this);
            playerMoves[1] = Move.CreateMove(moveType1, PlayerID.PLAYER1, this);

            moves = playerMoves;

            if (loadFromFile)
            {
                StartReadingFromFile();
            }
        }

        #endregion

        #region ReadingFromFile
        public void StartReadingFromFile()
        {
            moves = fileMoves;
        }

        public void FinishReadingFromFile()
        {
            moves = playerMoves;
            moves[0].CopyMove(fileMoves[0]);
            moves[1].CopyMove(fileMoves[1]);
        }
        #endregion

        #region Highlight
        public void SetHighlight(bool[,] possibleMoves)
        {
            if (moves[(int)TurnId] is HumanMove || (Simulation && !(moves[(int)TurnId] is FileMove)))
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